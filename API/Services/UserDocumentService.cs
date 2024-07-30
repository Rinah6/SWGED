using AutoMapper;
using API.Dto;
using API.Model;
using API.Data.Entities;
using API.Repositories;

namespace API.Services
{
    public class UserDocumentService
    {
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly MailService _emailService;

        public UserDocumentService(
            UserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            MailService emailService
        )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<List<Model.UserDocument>> CreateUserDocument(Model.Document document, Guid currentUserId, List<DocumentRecipientsDto> recipientsList, EmailData? emailData)
        {
            int step = 1;

            var recipients = new List<Model.UserDocument>();

            var clientURL = _configuration.GetValue<string>("ClientURL")!;

            foreach (var item in recipientsList)
            {
                var recipientInfo = await _userRepository.Get(item.Id);

                Model.UserDocument recipient = new()
                {
                    Role = item.Role,
                    Color = item.Color,
                    Message = item.Message,
                    Document = document,
                    IsTheCurrentStepTurn = step == 1,
                    Step = step,
                    Fields = _mapper.Map<List<Model.Field>>(item.Fields)
                };

                if (step == 2)
                {
                    List<string> sendemail = new List<string> { recipientInfo.Email };
                    Dictionary<string, string> parameterList = new Dictionary<string, string>
                     {
                         { "NOM", document.Title }
                     };
                    await _emailService.SendAppropriateMail(MailType.AValider, parameterList, sendemail);

                }

                //if (emailData != null && recipientInfo != null && user!.Email != recipientInfo.Email)
                //{
                //    var emailCredentials = _configuration.GetSection("Email").Get<EmailCredentials>()!;

                //    var body = $@"
                //        <p>{emailData.Object}</p>

                //        Vous êtes invité(e) à signer un document sur <a href=""{clientURL}"">SoftGED</a>.

                //        <pre>{emailData.Message}</pre>
                //    ";

                //    Console.WriteLine(body);

                //    await _emailService.SendEmail($"SoftGED - {emailData.Title}", body, new List<string> { recipientInfo.Email });
                //    //await _emailService.SendEmail($"SoftGED - {emailData.Title}", body, new string[] { emailCredentials.Mail });
                //}

                recipient.User = recipientInfo;

                recipients.Add(recipient);

                step += 1;
            }

            var user = await _userRepository.Get(currentUserId);

            var currentUser = new Model.UserDocument()
            {
                Document = document,
                Step = 0,
                Role = Data.DocumentRole.Signatory,
                User = user,
                IsTheCurrentStepTurn = false
            };

            recipients.Add(currentUser);

            return recipients;
        }

        public List<UserDocumentToAdd> AddUsersDocuments(Guid documentId, List<DocumentRecipientsDto> recipientsList)
        {
            int step = 1;

            var recipients = new List<UserDocumentToAdd>();

            foreach (var recipient in recipientsList)
            {
                var userDocumentToAdd = new UserDocumentToAdd
                {
                    UserId = recipient.Id,
                    DocumentId = documentId,
                    Role = recipient.Role,
                    Color = recipient.Color,
                    Message = recipient.Message,
                    IsTheCurrentStepTurn = step == 1,
                    Step = step,
                    Fields = _mapper.Map<List<Data.Entities.Field>>(recipient.Fields),
                    ProcessingDate = recipient.ProcessingDate
                };

                step += 1;

                recipients.Add(userDocumentToAdd);
            }

            return recipients;
        }

        public async Task LinkUserWithDocument(string code, List<UserRoleDocumentDto> users)
        {
            var tasks = new List<Task>();
            foreach (var u in users.OrderBy(x => x.Step))
            {
                tasks.Add(LinkUser(code, u));
            }
            await Task.WhenAll(tasks);
        }

        private async Task LinkUser(string Id, UserRoleDocumentDto u)
        {
            string password;
            Model.User? user = await _userRepository.GetByMail(u.Mail);

            if (user != null)
            {
                // await _userDocumentRepository.Create(new Model.UserDocument
                // {
                //     DocumentId = Id,
                //     UserId = user.Id,
                //     Role = u.Role,
                //     Step = u.Step,
                // });
            }
            else
            {
                password = Utils.Password.RandomPassword(u.Mail);

                user = await _userRepository.Create(new Model.User
                {
                    Email = u.Mail,
                    Password = Utils.Password.HashPassword(password),
                });

                // await _userDocumentRepository.Create(new Model.UserDocument
                // {
                //     DocumentId = Id,
                //     UserId = user.Id,
                //     Role = u.Role,
                //     Step = u.Step,
                // });
            }
        }

        private async Task<Guid> GetUserId(string email)
        {
            Model.User? user = await _userRepository.GetByMail(email);

            if (user == null)
            {
                var password = Utils.Password.RandomPassword(email);

                user = await _userRepository.Create(new Model.User
                {
                    Email = email,
                    Password = Utils.Password.HashPassword(password),
                });
            }

            return user.Id;
        }
    }
}
