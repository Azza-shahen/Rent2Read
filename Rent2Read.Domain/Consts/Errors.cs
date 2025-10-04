namespace Rent2Read.Domain.Consts
{
    public static class Errors
    {
        public const string RequiredField = "This field is required ⚠️";
        public const string MaxLength = "{PropertyName} cannot be more than {MaxLength} characters.";
        public const string MaxMinLength = "The {PropertyName} must be between {MinLength} and {MaxLength} characters long.";
        public const string Duplicated = "Another record with the same {0} already exists!❌";
        public const string DuplicatedBook = "A book with the same title and author already exists!";
        public const string NotAllowedExtension = "Only .png, .jpg, and .jpeg files are allowed.";
        public const string MaxSize = "📂File size cannot exceed 2 MB.";
        public const string NotAllowFutureDates = "📅 Date cannot be in the future.";
        public const string NotFoundSubscriber = "🙅 Subscriber not found.";
        public const string InvalidRange = "🔢{PropertyName} must be between {From} and {To}.";
        public const string ConfirmPasswordNotMatch = "🔑 Password and confirmation password do not match.";
        public const string WeakPassword = "🔒 Password must contain uppercase, lowercase, digit, special character, and be at least 6 characters.";
        public const string InvalidUsername = "👤 Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "🇬🇧 Only English letters are allowed.";
        public const string OnlyArabicLetters = "🇪🇬 Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "🔡 Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "🚫 Special characters are not allowed.";
        public const string InvalidMobileNumber = "📱 Invalid mobile number format.";
        public const string InvalidNationalId = "🆔 Invalid national ID.";
        public const string InvalidSerialNumber = "🔢 Invalid serial number.";
        public const string NotAvailableRental = "This book/copy is not available for rental.";
        public const string EmptyImage = "Please select an image.🖼️ ";
        public const string BlackListedSubscriber = "🚫 This subscriber is blacklisted.";
        public const string InactiveSubscriber = "⚠️ This subscriber account is inactive.";
        public const string MaxCopiesReached = "Maximum rental limit reached. No more copies can be added.";
        public const string CopyIsInRental = "📕This copy is already rented.";
        public const string RentalNotAllowedForBlackListed = "🚫 Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "⚠️ Rental cannot be extended until the subscriber renews their account.";
        public const string ExtendNotAllowed = "⏳ Rental extension is not allowed.";
        public const string PenaltyShouldBePaid = "💰 Penalty must be paid before proceeding.";
        public const string InvalidStartDate = "📅 Invalid start date.";
        public const string InvalidEndDate = "📅 Invalid end date.";
    }
}
