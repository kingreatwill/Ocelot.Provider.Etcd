namespace Ocelot.Provider.Etcd
{
    using Errors;

    public class UnableToSetConfigInConsulError : Error
    {
        public UnableToSetConfigInConsulError(string s) 
            : base(s, OcelotErrorCode.UnknownError)
        {
        }
    }
}
