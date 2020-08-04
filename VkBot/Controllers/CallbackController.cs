using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VkNet.Model;
using VkNet.Utils;
using System.Text;
using System.IO;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using System.Text.RegularExpressions;

namespace VkBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;
        private readonly IVkApi _vkApi;

     
        public CallbackController(IVkApi vkApi, IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }
        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {
            // Тип события
            switch (updates.Type)
            {
                // Ключ-подтверждение
                case "confirmation":
                {
                    return Ok(_configuration["Config:Confirmation"]);
                }

                    // Новое сообщение
                   
                case "message_new":
                    {
                        // Десериализация
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        HandleAsync(msg);
                      
                        break;
                    }
            }

            return Ok("ok");
        }
        async public void HandleAsync(Message msg)//обработка полученного сообщения
        {


            var tmp = msg.Text;
            StringBuilder stringToRead = new StringBuilder();
          

            string Answer="";
           // Regex[] Coms = new Regex[] {  new Regex(@"^/help\s?"), new Regex(@"^/Code\s?"), new Regex(@"^/Decode\s?") };
          

            switch (msg.Text)
            {
                case var someVal when new Regex(@"^/Help\s?", RegexOptions.IgnoreCase).IsMatch(someVal):
                    Answer = "/Help *smth* - вывести справку по командам\n"
                        + "/Code *smth* - перевести произвольную последовательноть *smth* в двоичный код\n"
                        + "/Decode *smth* - раскодировать бинарную последовательноть *smth* в текст, если она является кодированным текстом\n";
                    break;

                case var someVal when new Regex(@"^/Code\s?", RegexOptions.IgnoreCase).IsMatch(someVal):
                    Answer = CodeBin(msg.Text.Substring(new string("/Code\\s").Length));
                    break;
                case var someVal when new Regex(@"^/Decode\s?", RegexOptions.IgnoreCase).IsMatch(someVal):
                    Answer = CodeBin(msg.Text.Substring(new string("/Decode\\s").Length));
                    break;
                default:
                    Answer = "Ошибка, нераспознанная последовательность";
                    break;

            }

            await _vkApi.Messages.SendAsync(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = msg.PeerId.Value,
                Message = Answer
            });

        }
        private string CodeBin(string s)
        {
            return "Coded "+s;
        }
        private string DecodeBin(string s)
        {
            Regex IsBinaryCode = new Regex(@"^(([01]{8}\s?){2})+$");

            if (IsBinaryCode.IsMatch(s))
            {
                return "Decoded " + s;
            }
            else
            {
                return "Ошибка декодирования";
            }
        }
    }
}
