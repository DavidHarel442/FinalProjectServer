using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class MessageHandler
    {// this class handles the messages received
        /// <summary>
        /// property to use for checking login and register
        /// </summary>
        public LoginAndRegister loginAndRegister;
        /// <summary>
        /// property to help managing the messages, specifically to send feedbacks and updates
        /// </summary>
        private TcpClientSession clientSession;

        private SaveDrawings saveDrawings;

        public MessageHandler(TcpClientSession session)
        {
            clientSession = session;
            loginAndRegister = new LoginAndRegister(clientSession.communicationProtocol,clientSession);
            saveDrawings = new SaveDrawings();

        }
        /// <summary>
        /// handles message. 
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(TcpProtocolMessage message)
        {
            Console.WriteLine($"Handling message: Command={message.Command}, Username={message.Username}, Arguments={message.Arguments}");
            switch (message.Command)
            {
                case "Login":
                    string username = message.Arguments.Split('\t')[0];
                    string password = message.Arguments.Split('\t')[1];
                    loginAndRegister.CheckLogin(username, password);
                    break;
                case "Register":
                    loginAndRegister.Register(message.Arguments);
                    break;
                case "SendForgotPassword":
                    HandleForgotPassword(message);
                    break;
                case "ValidateCode":
                    HandleForgotPassword(message);
                    break;
                case "ChangePassword":
                    HandleForgotPassword(message);
                    break;
                case "SendAuthentication":
                    HandleAuthentication(false, message);
                    break;
                case "Verify":
                    HandleAuthentication(true, message);
                    break;
                case "RequestFullDrawingState":
                    clientSession.openedDrawing = true;
                    Console.WriteLine("goes in if");
                    ServerManager.tcpServer.DrawingManager.RequestedFullDrawingState(clientSession);
                    break;
                case "SendFullDrawingState":
                    string[] parts = message.Arguments.Split('\t');
                    if (parts.Length == 2)
                    {
                        string recipientIP = parts[0];
                        string imageData = parts[1];
                        ServerManager.tcpServer.DrawingManager.SendFullDrawingState(imageData, recipientIP);
                    }
                    break;
                case "DrawingAction":
                    ServerManager.tcpServer.BroadCastExceptOne("DrawingUpdate",message.Arguments,message.Username);
                    break;
                case "SaveDrawing":
                    try
                    {
                        string[] parts123 = message.Arguments.Split('\t');
                        if (parts123.Length >= 2)
                        {
                            string imageData = parts123[0];
                            string drawingName = parts123[1];

                            bool success = saveDrawings.SaveDrawing(message.Username, drawingName, imageData);

                            if (success)
                            {
                                clientSession.SendMessage("Success", "DrawingSaved");
                            }
                            else
                            {
                                clientSession.SendMessage("Issue", "FailedToSaveDrawing");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in SaveDrawing: {ex.Message}");
                        clientSession.SendMessage("Issue", "FailedToSaveDrawing");
                    }
                    break;
                    
                // Add new case for ListDrawings
                case "ListDrawings":
                    try
                    {
                        List<string> drawings = saveDrawings.GetUserDrawings(message.Username);
                        string drawingsJson = JsonConvert.SerializeObject(drawings);
                        clientSession.SendMessage("DrawingsList", drawingsJson);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in ListDrawings: {ex.Message}");
                        clientSession.SendMessage("Issue", "FailedToListDrawings");
                    }
                    break;

                // Add new case for LoadDrawing
                case "LoadDrawing":
                    try
                    {
                        string drawingName = message.Arguments;
                        string imageData = saveDrawings.LoadDrawing(message.Username, drawingName);
                        if (imageData != null)
                        {
                            // Send the drawing data to all clients using the existing FullDrawingState handler
                            ServerManager.tcpServer.BroadCast("FullDrawingState", imageData);

                            // Send success message to the client who requested the drawing
                            clientSession.SendMessage("Success", "DrawingLoaded");
                        }
                        else
                        {
                            clientSession.SendMessage("Issue", "DrawingNotFound");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in LoadDrawing: {ex.Message}");
                        clientSession.SendMessage("Issue", "FailedToLoadDrawing");
                    }
                    break;

                // Add new case for DeleteDrawing
                case "DeleteDrawing":
                    try
                    {
                        string drawingName = message.Arguments;
                        bool success = saveDrawings.DeleteDrawing(message.Username, drawingName);

                        if (success)
                        {
                            clientSession.SendMessage("Success", "DrawingDeleted");
                        }
                        else
                        {
                            clientSession.SendMessage("Issue", "FailedToDeleteDrawing");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in DeleteDrawing: {ex.Message}");
                        clientSession.SendMessage("Issue", "FailedToDeleteDrawing");
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// this property is incharge of the object capable of generating the captcha for lign
        /// </summary>
        private CaptchaGenerator captchaGenerator;
        /// <summary>
        /// this function will send code to mail for two step authentication and captcha for triple one. and verifies them 
        /// </summary>
        private void HandleAuthentication(bool shouldVerify, TcpProtocolMessage message)
        {
            if (!shouldVerify)
            {
                captchaGenerator = new CaptchaGenerator();
                Thread.Sleep(1000);
                string email = "";
                if (message.Arguments == "")// arguments will be empty if its a login situation
                {
                    email = loginAndRegister.c.GetEmailFromUsername(message.Username);
                }
                else
                {
                    if (loginAndRegister.CanRegister(message.Username))
                    {
                        email = message.Arguments;
                    }
                    else
                    {
                        clientSession.SendMessage("Issue", "the username already exists");
                        return;
                    }
                }
                loginAndRegister.SendSecondAuthentication(email);

                string captchaText = captchaGenerator.GenerateCaptchaText();
                byte[] captchaImage = captchaGenerator.GenerateCaptchaImage(captchaText);

                // Send CAPTCHA image to client
                clientSession.SendMessage("CaptchaImage", Convert.ToBase64String(captchaImage));
            }
            else
            {
                string[] parts = message.Arguments.Split('\t');
                string inputCode = parts[0];
                string inputCaptcha = parts[1];

                if (loginAndRegister.token != inputCode)
                {
                    clientSession.SendMessage("Issue", "WrongCode");
                }
                else if (!captchaGenerator.ValidateCaptcha(inputCaptcha))
                {
                    clientSession.SendMessage("Issue", "WrongCaptcha");
                }
                else
                {
                    // Authentication successful
                    clientSession.SendMessage("Success", "AuthenticationVerified");
                }
            }
        }
        /// <summary>
        /// handle the whole proccess message wise of the forgot password proccess
        /// </summary>
        /// <param name="message"></param>
        private void HandleForgotPassword(TcpProtocolMessage message)
        {
            switch (message.Command)
            {
                case "SendForgotPassword":
                    if (loginAndRegister.c.IsExist(message.Arguments))
                    {
                        string email = loginAndRegister.c.GetEmailFromUsername(message.Arguments);
                        loginAndRegister.SendForgotPassword(email);//sends a mail to the client who asked for it.(forgot password)
                    }
                    else
                    {
                        clientSession.SendMessage("Issue", "UsernameDoesntExist");// sends a feedback saying his username doesnt exist
                    }
                    break;
                case "ValidateCode":
                    string[] info = message.Arguments.Split('\t');
                    if (loginAndRegister.token == info[1])
                    {
                        clientSession.SendMessage("Success", "CodeValidated");//sends a feedback that his verification code is correct
                    }
                    else
                    {
                        clientSession.SendMessage("Issue", "WrongCode");//sends a feedback that his verification code was wrong
                    }
                    break;
                case "ChangePassword":
                    string[] info1 = message.Arguments.Split('\t');
                    string password = loginAndRegister.EncryptPassword(info1[1]);
                    loginAndRegister.c.InsertNewPassword(info1[0], password);
                    clientSession.SendMessage("Success", "PasswordChanged");//sends a feedback that his password was changed
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// handles the acceptence of username after encryption
        /// </summary>
        /// <param name="encryptedMessage"></param>
        public void HandleUsernameMessage(string message)
        {
            try
            {
                string command = message.Split('\n')[0];
                if (command == "USERNAME")
                {
                    string encodedUsername = message.Split('\n')[1].TrimEnd('\r');
                    
                        string decodedUsername = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsername));
                        Console.WriteLine($"Received username: {decodedUsername}");

                        if (ServerManager.tcpServer.SomeoneAlreadyConnected(decodedUsername))
                        {
                        clientSession.SendMessage("ERROR", "SomeoneAlreadyConnected");
                        }
                        else
                        {
                        clientSession._ClientNick = decodedUsername;
                        clientSession.isInitialConnectionComplete = true;
                            clientSession.SendMessage("UsernameAccepted", "UsernameAccepted");
                        }
                }
                else
                {
                    clientSession.SendMessage("ERROR", "ExpectedUsernameMessage");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling username message: {ex.Message}");
                clientSession.SendMessage("ERROR", "InvalidUsernameMessage");
            }
        }


    }
}
