namespace Rent2Read.Web.Core.Consts
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Archive = "Archive";
        public const string Reception = "Reception";
    }
    /* 
     *Grouping roles in one place
        -Instead of writing "Admin,"...as strings(hard-coded), you put them in a static class.
        -This way, if you want to change the role name, you only change it in one place.
    *Prevent typos
    *Easy to read and organize
    *Scalability and maintainability
    */
}
