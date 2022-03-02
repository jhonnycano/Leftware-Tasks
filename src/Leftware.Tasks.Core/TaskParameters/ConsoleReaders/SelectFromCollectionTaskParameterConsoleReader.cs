namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectFromCollectionTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectFromCollectionTaskParameter>
{
    private readonly ICollectionProvider _collectionProvider;
    private readonly CommonTaskInputHelper _inputHelper;

    public SelectFromCollectionTaskParameterConsoleReader(
        ICollectionProvider collectionProvider,
        CommonTaskInputHelper inputHelper
        )
    {
        _collectionProvider = collectionProvider;
        _inputHelper = inputHelper;
    }
    public override void Read(ConsoleReadContext context, SelectFromCollectionTaskParameter param)
    {
        /*
        string value;
        if (param.AllowManualEntry)
        {
            value = _inputHelper.GetStringFromCollection(param.Collection, param.Label, param.DefaultValue);
            if (string.IsNullOrEmpty(value))
            {
                context.IsCanceled = true;
                return;
            }
        }
        else
        {
            var configValue = _collectionProvider.SelectFromCollection(param.Collection, param.Label, param.DefaultValue, param.ExitValue);
            if (configValue == null)
            {
                context.IsCanceled = true;
                return;
            }
            value = param.UseKeyAsValue ? configValue.Name : configValue.Value;
        }

        context[param.Name] = value;
        */
    }
}
