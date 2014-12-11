namespace Keiser.MvxPlugins.Bluetooth
{
    using System.Text.RegularExpressions;

    public class Address
    {
        protected string _unformatedAddress;

        public Address(string address)
        {
            _unformatedAddress = RemoveFormatting(address);
        }

        public string Unformatted
        {
            get
            {
                return _unformatedAddress;
            }
        }

        public string ColonSeperated
        {
            get
            {
                return Regex.Replace(_unformatedAddress, "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})", "$1:$2:$3:$4:$5:$6");
            }
        }

        public bool IsEqual(string address)
        {
            return _unformatedAddress.Equals(RemoveFormatting(address));
        }

        protected string RemoveFormatting(string address)
        {
            return address.Replace(":", "");
        }
    }
}
