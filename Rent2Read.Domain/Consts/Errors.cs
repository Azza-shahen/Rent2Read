namespace Rent2Read.Domain.Consts
{
    public static class Errors
    {
        public const string RequiredField = "Required field";
        public const string MaxLength = "Length cannot be more than {1} characters";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exists!";
        public const string DuplicatedBook = "Book with the same Title is already exists with the same author!";
        public const string NotAllowedExtension = "Only .png, .jpg, .jpeg files are allowed!";
        public const string MaxSize = "File cannot be more than 2 MB!";
        public const string NotAllowFutureDates = "Date cannot be in the future!";
        public const string InvalidRange = "Edition number should be between 1 and 1000";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Password must contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least six characters long.";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidNationalId = "Invalid national Id.";
        public const string InvalidSerialNumber = "Invalid serial number.";
        public const string NotAvailableRental = "This book/copy is not available for rental.";
        public const string EmptyImage = "Please select an image.";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InactiveSubscriber = "This subscriber is inactive.";
        public const string MaxCopiesReached = "Maximum rental limit reached. No more copies can be added.";
        public const string CopyIsInRental = "This copy is already rentaled.";
        public const string RentalNotAllowedForBlackListed = "Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscriber before renewal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
        public const string InvalidStartDate = "The start date is invalid.";
        public const string InvalidEndDate = "The end date is invalid.";
    }
}
