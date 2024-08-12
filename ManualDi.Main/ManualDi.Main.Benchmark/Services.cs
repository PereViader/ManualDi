using Microsoft.Extensions.DependencyInjection;

namespace ManualDi.Main.Benchmark;

#pragma warning disable 9113
public class Service1();
public class Service2(Service1 service1);
public class Service3(Service2 service2);
public class Service4(Service3 service3);
public class Service5(Service4 service4);
public class Service6(Service5 service5);
public class Service7(Service6 service6);
public class Service8(Service7 service7);
public class Service9(Service8 service8);
public class Service10(Service9 service9);
public class Service11(Service10 service10);
public class Service12(Service11 service11);
public class Service13(Service12 service12);
public class Service14(Service13 service13);
public class Service15(Service14 service14);
public class Service16(Service15 service15);
public class Service17(Service16 service16);
public class Service18(Service17 service17);
public class Service19(Service18 service18);
public class Service20(Service19 service19);
public class Service21(Service20 service20);
public class Service22(Service21 service21);
public class Service23(Service22 service22);
public class Service24(Service23 service23);
public class Service25(Service24 service24);
public class Service26(Service25 service25);
public class Service27(Service26 service26);
public class Service28(Service27 service27);
public class Service29(Service28 service28);
public class Service30(Service29 service29);
public class Service31(Service30 service30);
public class Service32(Service31 service31);
public class Service33(Service32 service32);
public class Service34(Service33 service33);
public class Service35(Service34 service34);
public class Service36(Service35 service35);
public class Service37(Service36 service36);
public class Service38(Service37 service37);
public class Service39(Service38 service38);
public class Service40(Service39 service39);
public class Service41(Service40 service40);
public class Service42(Service41 service41);
public class Service43(Service42 service42);
public class Service44(Service43 service43);
public class Service45(Service44 service44);
public class Service46(Service45 service45);
public class Service47(Service46 service46);
public class Service48(Service47 service47);
public class Service49(Service48 service48);
public class Service50(Service49 service49);
public class Service51(Service50 service50);
public class Service52(Service51 service51);
public class Service53(Service52 service52);
public class Service54(Service53 service53);
public class Service55(Service54 service54);
public class Service56(Service55 service55);
public class Service57(Service56 service56);
public class Service58(Service57 service57);
public class Service59(Service58 service58);
public class Service60(Service59 service59);
public class Service61(Service60 service60);
public class Service62(Service61 service61);
public class Service63(Service62 service62);
public class Service64(Service63 service63);
public class Service65(Service64 service64);
public class Service66(Service65 service65);
public class Service67(Service66 service66);
public class Service68(Service67 service67);
public class Service69(Service68 service68);
public class Service70(Service69 service69);
public class Service71(Service70 service70);
public class Service72(Service71 service71);
public class Service73(Service72 service72);
public class Service74(Service73 service73);
public class Service75(Service74 service74);
public class Service76(Service75 service75);
public class Service77(Service76 service76);
public class Service78(Service77 service77);
public class Service79(Service78 service78);
public class Service80(Service79 service79);
public class Service81(Service80 service80);
public class Service82(Service81 service81);
public class Service83(Service82 service82);
public class Service84(Service83 service83);
public class Service85(Service84 service84);
public class Service86(Service85 service85);
public class Service87(Service86 service86);
public class Service88(Service87 service87);
public class Service89(Service88 service88);
public class Service90(Service89 service89);
public class Service91(Service90 service90);
public class Service92(Service91 service91);
public class Service93(Service92 service92);
public class Service94(Service93 service93);
public class Service95(Service94 service94);
public class Service96(Service95 service95);
public class Service97(Service96 service96);
public class Service98(Service97 service97);
public class Service99(Service98 service98);
public class Service100(Service99 service99);
#pragma warning restore 9113


public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddServices(this ServiceCollection serviceCollection)
    {
        serviceCollection
            .AddTransient<Service1>()
            .AddTransient<Service2>()
            .AddTransient<Service3>()
            .AddTransient<Service4>()
            .AddTransient<Service5>()
            .AddTransient<Service6>()
            .AddTransient<Service7>()
            .AddTransient<Service8>()
            .AddTransient<Service9>()
            .AddTransient<Service10>()
            .AddTransient<Service11>()
            .AddTransient<Service12>()
            .AddTransient<Service13>()
            .AddTransient<Service14>()
            .AddTransient<Service15>()
            .AddTransient<Service16>()
            .AddTransient<Service17>()
            .AddTransient<Service18>()
            .AddTransient<Service19>()
            .AddTransient<Service20>()
            .AddTransient<Service21>()
            .AddTransient<Service22>()
            .AddTransient<Service23>()
            .AddTransient<Service24>()
            .AddTransient<Service25>()
            .AddTransient<Service26>()
            .AddTransient<Service27>()
            .AddTransient<Service28>()
            .AddTransient<Service29>()
            .AddTransient<Service30>()
            .AddTransient<Service31>()
            .AddTransient<Service32>()
            .AddTransient<Service33>()
            .AddTransient<Service34>()
            .AddTransient<Service35>()
            .AddTransient<Service36>()
            .AddTransient<Service37>()
            .AddTransient<Service38>()
            .AddTransient<Service39>()
            .AddTransient<Service40>()
            .AddTransient<Service41>()
            .AddTransient<Service42>()
            .AddTransient<Service43>()
            .AddTransient<Service44>()
            .AddTransient<Service45>()
            .AddTransient<Service46>()
            .AddTransient<Service47>()
            .AddTransient<Service48>()
            .AddTransient<Service49>()
            .AddTransient<Service50>()
            .AddTransient<Service51>()
            .AddTransient<Service52>()
            .AddTransient<Service53>()
            .AddTransient<Service54>()
            .AddTransient<Service55>()
            .AddTransient<Service56>()
            .AddTransient<Service57>()
            .AddTransient<Service58>()
            .AddTransient<Service59>()
            .AddTransient<Service60>()
            .AddTransient<Service61>()
            .AddTransient<Service62>()
            .AddTransient<Service63>()
            .AddTransient<Service64>()
            .AddTransient<Service65>()
            .AddTransient<Service66>()
            .AddTransient<Service67>()
            .AddTransient<Service68>()
            .AddTransient<Service69>()
            .AddTransient<Service70>()
            .AddTransient<Service71>()
            .AddTransient<Service72>()
            .AddTransient<Service73>()
            .AddTransient<Service74>()
            .AddTransient<Service75>()
            .AddTransient<Service76>()
            .AddTransient<Service77>()
            .AddTransient<Service78>()
            .AddTransient<Service79>()
            .AddTransient<Service80>()
            .AddTransient<Service81>()
            .AddTransient<Service82>()
            .AddTransient<Service83>()
            .AddTransient<Service84>()
            .AddTransient<Service85>()
            .AddTransient<Service86>()
            .AddTransient<Service87>()
            .AddTransient<Service88>()
            .AddTransient<Service89>()
            .AddTransient<Service90>()
            .AddTransient<Service91>()
            .AddTransient<Service92>()
            .AddTransient<Service93>()
            .AddTransient<Service94>()
            .AddTransient<Service95>()
            .AddTransient<Service96>()
            .AddTransient<Service97>()
            .AddTransient<Service98>()
            .AddTransient<Service99>()
            .AddSingleton<Service100>();
        return serviceCollection;
    }
}

public static class ManualDiInstallerExtensions
{
    public static DiContainerBindings InstallServices(this DiContainerBindings b)
    {
        b.Bind<Service1>().Default().Transient().FromConstructor();
        b.Bind<Service2>().Default().Transient().FromConstructor();
        b.Bind<Service3>().Default().Transient().FromConstructor();
        b.Bind<Service4>().Default().Transient().FromConstructor();
        b.Bind<Service5>().Default().Transient().FromConstructor();
        b.Bind<Service6>().Default().Transient().FromConstructor();
        b.Bind<Service7>().Default().Transient().FromConstructor();
        b.Bind<Service8>().Default().Transient().FromConstructor();
        b.Bind<Service9>().Default().Transient().FromConstructor();
        b.Bind<Service10>().Default().Transient().FromConstructor();
        b.Bind<Service11>().Default().Transient().FromConstructor();
        b.Bind<Service12>().Default().Transient().FromConstructor();
        b.Bind<Service13>().Default().Transient().FromConstructor();
        b.Bind<Service14>().Default().Transient().FromConstructor();
        b.Bind<Service15>().Default().Transient().FromConstructor();
        b.Bind<Service16>().Default().Transient().FromConstructor();
        b.Bind<Service17>().Default().Transient().FromConstructor();
        b.Bind<Service18>().Default().Transient().FromConstructor();
        b.Bind<Service19>().Default().Transient().FromConstructor();
        b.Bind<Service20>().Default().Transient().FromConstructor();
        b.Bind<Service21>().Default().Transient().FromConstructor();
        b.Bind<Service22>().Default().Transient().FromConstructor();
        b.Bind<Service23>().Default().Transient().FromConstructor();
        b.Bind<Service24>().Default().Transient().FromConstructor();
        b.Bind<Service25>().Default().Transient().FromConstructor();
        b.Bind<Service26>().Default().Transient().FromConstructor();
        b.Bind<Service27>().Default().Transient().FromConstructor();
        b.Bind<Service28>().Default().Transient().FromConstructor();
        b.Bind<Service29>().Default().Transient().FromConstructor();
        b.Bind<Service30>().Default().Transient().FromConstructor();
        b.Bind<Service31>().Default().Transient().FromConstructor();
        b.Bind<Service32>().Default().Transient().FromConstructor();
        b.Bind<Service33>().Default().Transient().FromConstructor();
        b.Bind<Service34>().Default().Transient().FromConstructor();
        b.Bind<Service35>().Default().Transient().FromConstructor();
        b.Bind<Service36>().Default().Transient().FromConstructor();
        b.Bind<Service37>().Default().Transient().FromConstructor();
        b.Bind<Service38>().Default().Transient().FromConstructor();
        b.Bind<Service39>().Default().Transient().FromConstructor();
        b.Bind<Service40>().Default().Transient().FromConstructor();
        b.Bind<Service41>().Default().Transient().FromConstructor();
        b.Bind<Service42>().Default().Transient().FromConstructor();
        b.Bind<Service43>().Default().Transient().FromConstructor();
        b.Bind<Service44>().Default().Transient().FromConstructor();
        b.Bind<Service45>().Default().Transient().FromConstructor();
        b.Bind<Service46>().Default().Transient().FromConstructor();
        b.Bind<Service47>().Default().Transient().FromConstructor();
        b.Bind<Service48>().Default().Transient().FromConstructor();
        b.Bind<Service49>().Default().Transient().FromConstructor();
        b.Bind<Service50>().Default().Transient().FromConstructor();
        b.Bind<Service51>().Default().Transient().FromConstructor();
        b.Bind<Service52>().Default().Transient().FromConstructor();
        b.Bind<Service53>().Default().Transient().FromConstructor();
        b.Bind<Service54>().Default().Transient().FromConstructor();
        b.Bind<Service55>().Default().Transient().FromConstructor();
        b.Bind<Service56>().Default().Transient().FromConstructor();
        b.Bind<Service57>().Default().Transient().FromConstructor();
        b.Bind<Service58>().Default().Transient().FromConstructor();
        b.Bind<Service59>().Default().Transient().FromConstructor();
        b.Bind<Service60>().Default().Transient().FromConstructor();
        b.Bind<Service61>().Default().Transient().FromConstructor();
        b.Bind<Service62>().Default().Transient().FromConstructor();
        b.Bind<Service63>().Default().Transient().FromConstructor();
        b.Bind<Service64>().Default().Transient().FromConstructor();
        b.Bind<Service65>().Default().Transient().FromConstructor();
        b.Bind<Service66>().Default().Transient().FromConstructor();
        b.Bind<Service67>().Default().Transient().FromConstructor();
        b.Bind<Service68>().Default().Transient().FromConstructor();
        b.Bind<Service69>().Default().Transient().FromConstructor();
        b.Bind<Service70>().Default().Transient().FromConstructor();
        b.Bind<Service71>().Default().Transient().FromConstructor();
        b.Bind<Service72>().Default().Transient().FromConstructor();
        b.Bind<Service73>().Default().Transient().FromConstructor();
        b.Bind<Service74>().Default().Transient().FromConstructor();
        b.Bind<Service75>().Default().Transient().FromConstructor();
        b.Bind<Service76>().Default().Transient().FromConstructor();
        b.Bind<Service77>().Default().Transient().FromConstructor();
        b.Bind<Service78>().Default().Transient().FromConstructor();
        b.Bind<Service79>().Default().Transient().FromConstructor();
        b.Bind<Service80>().Default().Transient().FromConstructor();
        b.Bind<Service81>().Default().Transient().FromConstructor();
        b.Bind<Service82>().Default().Transient().FromConstructor();
        b.Bind<Service83>().Default().Transient().FromConstructor();
        b.Bind<Service84>().Default().Transient().FromConstructor();
        b.Bind<Service85>().Default().Transient().FromConstructor();
        b.Bind<Service86>().Default().Transient().FromConstructor();
        b.Bind<Service87>().Default().Transient().FromConstructor();
        b.Bind<Service88>().Default().Transient().FromConstructor();
        b.Bind<Service89>().Default().Transient().FromConstructor();
        b.Bind<Service90>().Default().Transient().FromConstructor();
        b.Bind<Service91>().Default().Transient().FromConstructor();
        b.Bind<Service92>().Default().Transient().FromConstructor();
        b.Bind<Service93>().Default().Transient().FromConstructor();
        b.Bind<Service94>().Default().Transient().FromConstructor();
        b.Bind<Service95>().Default().Transient().FromConstructor();
        b.Bind<Service96>().Default().Transient().FromConstructor();
        b.Bind<Service97>().Default().Transient().FromConstructor();
        b.Bind<Service98>().Default().Transient().FromConstructor();
        b.Bind<Service99>().Default().Transient().FromConstructor();
        b.Bind<Service100>().Default().Single().FromConstructor();
        return b;
    }
}