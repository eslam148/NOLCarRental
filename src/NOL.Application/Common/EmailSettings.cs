using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOL.Application.Common
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "sandbox.smtp.mailtrap.io";
        public int SmtpPort { get; set; } = 2525;
        public string SenderEmail { get; set; } = "info@nol.com";
        public string SenderName { get; set; } = "NOL";
        public string Username { get; set; } = "cae84e2257385f";
        public string Password { get; set; } = "685f9ced75eb7f";
        public bool EnableSsl { get; set; } = true;
    }
}
//public string SmtpServer { get; set; } = "sandbox.smtp.mailtrap.io";
//public int SmtpPort { get; set; } = 2525;
//public string SenderEmail { get; set; } = "info@nol.com";
//public string SenderName { get; set; } = "NOL";
//public string Username { get; set; } = "cae84e2257385f";
//public string Password { get; set; } = "685f9ced75eb7f";
//public bool EnableSsl { get; set; } = true;



//public string SmtpServer { get; set; } = "sandbox.smtp.mailtrap.io";
//public int SmtpPort { get; set; } = 2525;
//public string SenderEmail { get; set; } = "islam12476794@gmail.com";
//public string SenderName { get; set; } = "Eslam Mohamed Amir";
//public string Username { get; set; } = "75d7b89b800244";
//public string Password { get; set; } = "b0b72ad703383e";
//public bool EnableSsl { get; set; } = true;