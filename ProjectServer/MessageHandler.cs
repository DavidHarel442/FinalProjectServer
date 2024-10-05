using System;
using System.Collections.Generic;
using System.Linq;
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
        public MessageHandler(TcpClientSession session)
        {
            clientSession = session;
            loginAndRegister = new LoginAndRegister(clientSession.communicationProtocol,clientSession);
        }
        /// <summary>
        /// handles message. 
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(TcpCommunicationProtocol message)
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
                default:
                    break;
            }
        }
        private CaptchaGenerator captchaGenerator = new CaptchaGenerator();
        /// <summary>
        /// this function will send code to mail for two step authentication and captcha for triple one. and verifies them 
        /// </summary>
        private void HandleAuthentication(bool shouldVerify, TcpCommunicationProtocol message)
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
        private void HandleForgotPassword(TcpCommunicationProtocol message)
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
