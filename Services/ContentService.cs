using CoreAudit.Models;

namespace CoreAudit.Services;

/// <summary>
/// Supplies the static Arabic marketing content (services, principles, process steps).
/// Centralising it here keeps controllers/views thin and makes the copy easy to update.
/// </summary>
public interface IContentService
{
    IReadOnlyList<ServiceItem> GetServices();
    IReadOnlyList<PrincipleItem> GetPrinciples();
    IReadOnlyList<ProcessStep> GetProcessSteps();
}

public class ContentService : IContentService
{
    private static readonly List<ServiceItem> Services = new()
    {
        new ServiceItem
        {
            Slug = "external-audit",
            TitleAr = "المراجعة الخارجية",
            TitleEn = "External Audit",
            Icon = "bi-clipboard-check",
            DescriptionAr = "خدمات مراجعة مهنية تهدف إلى دعم موثوقية ووضوح التقارير المالية وتعزيز الثقة في البيانات المعروضة."
        },
        new ServiceItem
        {
            Slug = "accounting-bookkeeping",
            TitleAr = "الخدمات المحاسبية ومسك الدفاتر",
            TitleEn = "Accounting and Bookkeeping",
            Icon = "bi-journal-text",
            DescriptionAr = "تنظيم السجلات المحاسبية ومسك الدفاتر وإدارة المعلومات المالية بطريقة دقيقة ومنظمة."
        },
        new ServiceItem
        {
            Slug = "tax-financial-consulting",
            TitleAr = "الاستشارات الضريبية والمالية",
            TitleEn = "Tax and Financial Consulting",
            Icon = "bi-graph-up-arrow",
            DescriptionAr = "الدعم في المسائل الضريبية والتخطيط المالي، بما يتوافق مع الأنظمة المعمول بها ونطاق خدمات المكتب."
        },
        new ServiceItem
        {
            Slug = "internal-audit-risk",
            TitleAr = "المراجعة الداخلية وإدارة المخاطر",
            TitleEn = "Internal Audit and Risk Management",
            Icon = "bi-shield-check",
            DescriptionAr = "دعم تقييم العمليات الداخلية والضوابط الرقابية والمخاطر المحتملة التي قد تواجه الأعمال."
        },
        new ServiceItem
        {
            Slug = "financial-reporting",
            TitleAr = "إعداد التقارير المالية",
            TitleEn = "Financial Reporting",
            Icon = "bi-file-earmark-bar-graph",
            DescriptionAr = "تقارير مالية واضحة ومنظمة تساعد الأعمال على فهم مركزها المالي بشكل أفضل."
        },
        new ServiceItem
        {
            Slug = "business-consulting",
            TitleAr = "استشارات الأعمال",
            TitleEn = "Business Consulting",
            Icon = "bi-lightbulb",
            DescriptionAr = "رؤى مالية وتجارية تدعم عمليات التخطيط واتخاذ القرار على مستوى الأعمال."
        },
        new ServiceItem
        {
            Slug = "judicial-support",
            TitleAr = "الدعم القضائي",
            TitleEn = "Judicial Support",
            Icon = "bi-bank2",
            DescriptionAr = "دعم محاسبي وفني للقضايا والنزاعات يشمل تقارير الخبرة وحصر الموجودات والمديونيات.",
            IntroAr = "نقدم في كور أوديت دعمًا محاسبيًا وفنيًا يساعد أطراف القضايا والنزاعات على فهم الجوانب المالية بوضوح وتنظيم المستندات والبيانات ذات الصلة. يعتمد عملنا على الدقة والحياد والسرية، ونعرض النتائج في تقارير منظمة تسهّل الرجوع إليها. وتقتصر خدماتنا على الجانب المحاسبي والفني، ولا تغني عن المشورة القانونية المتخصصة.",
            LeadInAr = "تقديم خدمات الدعم المحاسبي والفني للقضايا والنزاعات، من خلال:",
            BulletsAr = new[]
            {
                "إعداد تقارير الخبرة المحاسبية للنزاعات التجارية.",
                "حصر الموجودات والمديونيات.",
                "إعداد تقارير الاختلاسات والتعاملات غير المشروعة."
            },
            CtaHeadingAr = "هل لديك قضية أو نزاع يحتاج إلى دعم محاسبي؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة احتياجاتك وطلب استشارة مهنية بسرية تامة."
        },
    };

    private static readonly List<PrincipleItem> Principles = new()
    {
        new PrincipleItem { Icon = "bi-bullseye",      TitleAr = "الدقة والاهتمام بالتفاصيل", DescriptionAr = "نولي اهتمامًا دقيقًا لكل تفصيل لضمان جودة العمل." },
        new PrincipleItem { Icon = "bi-eye",           TitleAr = "الشفافية والوضوح",          DescriptionAr = "نلتزم بالوضوح الكامل في التواصل والتقارير." },
        new PrincipleItem { Icon = "bi-lock",          TitleAr = "السرية وحماية المعلومات",   DescriptionAr = "نحافظ على خصوصية معلومات عملائنا وحمايتها." },
        new PrincipleItem { Icon = "bi-people",        TitleAr = "الاهتمام باحتياجات العملاء", DescriptionAr = "نضع احتياجات العميل في صميم عملنا المهني." },
        new PrincipleItem { Icon = "bi-file-text",     TitleAr = "التقارير المنظمة",          DescriptionAr = "نقدم تقارير مرتبة وسهلة الفهم." },
        new PrincipleItem { Icon = "bi-chat-dots",     TitleAr = "التواصل المهني",            DescriptionAr = "نحرص على تواصل مهني ومستمر مع العملاء." },
    };

    private static readonly List<ProcessStep> ProcessSteps = new()
    {
        new ProcessStep { Number = 1, Icon = "bi-search",        TitleAr = "فهم احتياجاتك",       DescriptionAr = "نبدأ بفهم طبيعة أعمالك ومتطلباتك بشكل دقيق." },
        new ProcessStep { Number = 2, Icon = "bi-list-check",    TitleAr = "تحديد نطاق العمل",    DescriptionAr = "نتفق معك على الخدمة المطلوبة ونطاق العمل بوضوح." },
        new ProcessStep { Number = 3, Icon = "bi-gear-wide-connected", TitleAr = "تنفيذ العمل المهني", DescriptionAr = "ننفذ العمل المتفق عليه وفق أعلى المعايير المهنية." },
        new ProcessStep { Number = 4, Icon = "bi-clipboard-data", TitleAr = "عرض النتائج والتوصيات", DescriptionAr = "نعرض النتائج والتوصيات ذات الصلة بشكل واضح ومنظم." },
    };

    public IReadOnlyList<ServiceItem> GetServices() => Services;
    public IReadOnlyList<PrincipleItem> GetPrinciples() => Principles;
    public IReadOnlyList<ProcessStep> GetProcessSteps() => ProcessSteps;
}
