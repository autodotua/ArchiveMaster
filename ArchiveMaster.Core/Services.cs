using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster;

public static class Services
{
    //private static IServiceProvider provider;

    //public static void Initialize(IServiceCollection services)
    //{
    //    if (Builder != null)
    //    {
    //        throw new InvalidOperationException("�Ѿ�������ʼ��");
    //    }
    //    Builder = services ?? throw new ArgumentNullException();
    //}
    //public static IServiceCollection Builder { get; private set; }
    //public static IServiceProvider Provider
    //{
    //    get => provider??throw new InvalidOperationException("��δ��ʼ��");
    //    private set => provider = value;
    //}

    //public static void BuildServiceProvider()
    //{
    //    Provider = Builder.BuildServiceProvider();
    //}
    public static void Initialize(IServiceProvider services)
    {
        if (Provider != null)
        {
            throw new InvalidOperationException("�Ѿ�������ʼ��");
        }
        Provider = services ?? throw new ArgumentNullException();
    }
    public static IServiceProvider Provider { get; private set; }

}