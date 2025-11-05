using AuthApiBackend.DTOs;

namespace AuthApiBackend.Utilities
{

    public static class RoleFormat
    {

        public static string Format(string role)
        {
            var roleFormat = string.Empty;

            for (int i = 0; i < role.Length; i++)
            {
                if (i == 0)
                    roleFormat += role[i].ToString().ToUpper();
                else
                    roleFormat += role[i].ToString().ToLower();
            }

            return roleFormat;
        }

    }

}