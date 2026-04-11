namespace RawPowerLabs.DynamicAI
{
    public interface ITextModule
    {
        TextModuleInput CreateInput();
        TextModuleResult? Invoke(TextModuleInvokeParameters parameters, TextModuleInput input);
    }
}
