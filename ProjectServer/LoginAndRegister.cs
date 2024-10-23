using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    public class LoginAndRegister
    {// this class manages the login and register phase
        /// <summary>
        /// this property 'Alphabet' contains the letters of the alphabet. camel case and upper case
        /// </summary>
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        /// <summary>
        /// this property is a static property which is a random used to create the token
        /// </summary>
        private static readonly Random Random = new Random();
        /// <summary>
        /// this property 'token' is a token generated with 6 digits.
        /// and the user recieves the token to his gmail and he has to put it in and then after verification he can change his password
        /// </summary>
        public string token;
        /// <summary>
        /// this property 'c' is an object which with it you communicate with the Sql Database
        /// </summary>
        public SqlConnection c = new SqlConnection();
        /// <summary>
        /// this property allows the server to communicate with its clients.
        /// </summary>
        public TcpCommunicationProtocol communicationProtocol = null;




        /// <summary>
        /// object that contains the current client that the server does actions on currently.
        /// </summary>
        public TcpClientSession clientSession = null;
        /// <summary>
        /// constructor. receives a communication protocol object and equals it to the property that is null
        /// </summary>
        /// <param name="communicationProtocol"></param>
        public LoginAndRegister(TcpCommunicationProtocol communicationProtocol, TcpClientSession clientSession)
        {
            this.communicationProtocol = communicationProtocol;
            this.clientSession = clientSession;
        }
        /// <summary>
        /// this function recieves all the information about a client and registering it in the database
        /// </summary>
        /// <param name="allTheInfo"></param>
        /// <param name="Manager"></param>
        public void Register(string allTheInfo)
        {
            string[] info = allTheInfo.Split('\t');
            if (c.IsExist(info[0]))
            { 
                clientSession.SendMessage("Issue", "the username already exists");
            }
            else
            {
                string EncryptedPassword = EncryptPassword(info[1]);
                c.InsertNewUser(info[0], EncryptedPassword, info[2], info[3], info[4], info[5], info[6]);          
                clientSession.SendMessage("Registered", info[2]);
            }
        }
        /// <summary>
        /// this function checks if the client can register in the system , it checks if the username he tried to put in can be used
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool CanRegister(string username)
        {
            if (c.IsExist(username))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// this function checks if the recieved name and password exist in the database. and if it does it sends an approval and the first name of the user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="Manager"></param>
        public void CheckLogin(string name, string password)
        {
            string EncryptedPassword = EncryptPassword(password);
            bool LoginCorrect = c.LoginCheck(name, EncryptedPassword);
            string FirstName = c.GetFirstName(name);
            if (!LoginCorrect)
            {
                clientSession.SendMessage("Issue", "Not Logged In");
            }       
            else
            {
                clientSession.SendMessage("LoggedIn", FirstName);              
            }
        }




        /// <summary>
        /// sends the mail to the user who asked to change the password. it sends him code which he needs to put to validate that it is in fact him
        /// </summary>
        /// <param name="mailTo"></param>
        public void SendForgotPassword(string mailTo)
        {
            this.token = GenerateToken(6);
            MailSender obj = new MailSender();
            obj.SendForgotPasswordMail(mailTo, token);
        }
        /// <summary>
        /// sends the mail to the user who asked to change the login/register. it sends him code which he needs to put to validate that it is in fact him
        /// </summary>
        /// <param name="mailTo"></param>
        public void SendSecondAuthentication(string mailTo)
        {
            this.token = GenerateToken(6);
            MailSender obj = new MailSender();
            obj.SendTwoStepAuthenticationMail(mailTo, token);
        }
        /// <summary>
        /// this function generates a unique token that will be used for forgot password
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateToken(int length)
        {
            return GenerateToken(Alphabet, length);
        }

        /// <summary>
        /// this function is called for in the function 'GenerateToken'. it generate the Unique token. 6 digits and contains all the letters in the Alphabet.
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateToken(string characters, int length)
        {
            return new string(Enumerable.Range(0, length).Select(num => characters[Random.Next() % characters.Length]).ToArray());
        }

        /// <summary>
        /// this function recieves a password and encrypts it using the 'SHA1' encryption
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncryptPassword(string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }
    }
}
