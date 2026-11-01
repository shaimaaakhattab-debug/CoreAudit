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
            ImagePath = "/images/services/external-audit.svg",
            DescriptionAr = "خدمات مراجعة مهنية تهدف إلى دعم موثوقية ووضوح التقارير المالية وتعزيز الثقة في البيانات المعروضة.",
            IntroAr = "نقدم في كور أوديت خدمات مراجعة مهنية تهدف إلى دعم موثوقية ووضوح التقارير المالية وتعزيز الثقة في البيانات المعروضة. نعمل بدقة وحياد وسرية، ونعرض ملاحظاتنا بصورة منظمة وواضحة.",
            DetailParagraphsAr = new[]
            {
                "تبدأ الخدمة بفهم طبيعة عملك وتحديد نطاق العمل المتفق عليه.",
                "يجري بعد ذلك فحص السجلات والبيانات المالية ذات الصلة ضمن هذا النطاق، ثم تعرض النتائج والملاحظات في تقرير مكتوب."
            },
            LeadInAr = "تشمل الخدمة بحسب نطاق العمل المتفق عليه:",
            BulletsAr = new[]
            {
                "مراجعة القوائم والبيانات المالية ضمن نطاق العمل المتفق عليه.",
                "فحص السجلات والمستندات المحاسبية ذات الصلة.",
                "مناقشة الملاحظات مع الإدارة المعنية.",
                "عرض النتائج في تقرير منظم وواضح."
            },
            DeliverablesAr = new[]
            {
                "تقرير مكتوب بنتائج المراجعة.",
                "ملاحظات وتوصيات ذات صلة بنطاق العمل."
            },
            CtaHeadingAr = "هل تحتاج إلى مراجعة تدعم وضوح تقاريرك المالية؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة احتياجاتك وتحديد نطاق العمل المناسب."
        },
        new ServiceItem
        {
            Slug = "accounting-bookkeeping",
            TitleAr = "الخدمات المحاسبية ومسك الدفاتر",
            TitleEn = "Accounting and Bookkeeping",
            Icon = "bi-journal-text",
            ImagePath = "/images/services/accounting-bookkeeping.svg",
            DescriptionAr = "تنظيم السجلات المحاسبية ومسك الدفاتر وإدارة المعلومات المالية بطريقة دقيقة ومنظمة.",
            IntroAr = "نساعدك في كور أوديت على تنظيم سجلاتك المحاسبية وإدارة معلوماتك المالية بطريقة دقيقة ومنظمة، بما يمنحك صورة واضحة عن أعمالك ويسهّل الرجوع إلى بياناتك.",
            DetailParagraphsAr = new[]
            {
                "تعتمد الخدمة على تسجيل العمليات المالية وتبويبها وحفظ سجلاتها بشكل مرتب.",
                "يحدد نطاق الخدمة وتكرارها بالاتفاق معك بحسب طبيعة نشاطك."
            },
            LeadInAr = "تشمل الخدمة بحسب الاتفاق:",
            BulletsAr = new[]
            {
                "مسك الدفاتر والسجلات المحاسبية.",
                "تسجيل وتبويب العمليات المالية.",
                "تنظيم المستندات والمعلومات المالية.",
                "مراجعة السجلات للتأكد من اتساقها ووضوحها."
            },
            DeliverablesAr = new[]
            {
                "سجلات محاسبية منظمة.",
                "كشوفات وتقارير مالية دورية بحسب الاتفاق."
            },
            CtaHeadingAr = "هل ترغب في تنظيم سجلاتك المحاسبية؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة احتياجاتك وتحديد نطاق الخدمة المناسب."
        },
        new ServiceItem
        {
            Slug = "tax-financial-consulting",
            TitleAr = "الاستشارات الضريبية والمالية",
            TitleEn = "Tax and Financial Consulting",
            Icon = "bi-graph-up-arrow",
            ImagePath = "/images/services/tax-financial-consulting.svg",
            DescriptionAr = "الدعم في المسائل الضريبية والتخطيط المالي، بما يتوافق مع الأنظمة المعمول بها ونطاق خدمات المكتب.",
            IntroAr = "نقدم في كور أوديت دعمًا في المسائل الضريبية والتخطيط المالي بما يتوافق مع الأنظمة المعمول بها ونطاق خدمات المكتب، لمساعدتك على فهم وضعك المالي واتخاذ قرارات أكثر وضوحًا.",
            DetailParagraphsAr = new[]
            {
                "تُقدَّم الاستشارة بحسب المعلومات والمستندات التي يزودنا بها العميل.",
                "تقتصر الخدمة على نطاق خدمات المكتب المتفق عليه."
            },
            LeadInAr = "يشمل الدعم بحسب نطاق العمل المتفق عليه:",
            BulletsAr = new[]
            {
                "مناقشة المسائل الضريبية المتعلقة بنشاطك.",
                "المساعدة في التخطيط المالي.",
                "مراجعة المعلومات والمستندات ذات الصلة.",
                "توضيح الاعتبارات المالية قبل اتخاذ القرار."
            },
            DeliverablesAr = new[]
            {
                "ملاحظات وتوصيات مكتوبة بحسب نطاق الاستشارة."
            },
            CtaHeadingAr = "هل لديك استفسار ضريبي أو مالي؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة استفسارك وطلب استشارة."
        },
        new ServiceItem
        {
            Slug = "internal-audit-risk",
            TitleAr = "المراجعة الداخلية وإدارة المخاطر",
            TitleEn = "Internal Audit and Risk Management",
            Icon = "bi-shield-check",
            ImagePath = "/images/services/internal-audit-risk.svg",
            DescriptionAr = "دعم تقييم العمليات الداخلية والضوابط الرقابية والمخاطر المحتملة التي قد تواجه الأعمال.",
            IntroAr = "نساعدك في كور أوديت على تقييم العمليات الداخلية والضوابط الرقابية وفهم المخاطر المحتملة التي قد تواجه أعمالك، وعرض الملاحظات بصورة منظمة وواضحة.",
            DetailParagraphsAr = new[]
            {
                "يتم تحديد العمليات محل التقييم بالاتفاق معك.",
                "تعرض النتائج والتوصيات ذات الصلة بشكل واضح ومنظم."
            },
            LeadInAr = "تشمل الخدمة بحسب نطاق العمل المتفق عليه:",
            BulletsAr = new[]
            {
                "تقييم العمليات الداخلية المتفق عليها.",
                "مراجعة الضوابط الرقابية القائمة.",
                "تحديد المخاطر المحتملة وعرضها.",
                "تقديم توصيات ذات صلة بنتائج التقييم."
            },
            DeliverablesAr = new[]
            {
                "تقرير بنتائج التقييم والملاحظات.",
                "توصيات مرتبطة بالمخاطر والضوابط المرصودة."
            },
            CtaHeadingAr = "هل تحتاج إلى تقييم عملياتك الداخلية؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة نطاق العمل المناسب لأعمالك."
        },
        new ServiceItem
        {
            Slug = "financial-reporting",
            TitleAr = "إعداد التقارير المالية",
            TitleEn = "Financial Reporting",
            Icon = "bi-file-earmark-bar-graph",
            ImagePath = "/images/services/financial-reporting.svg",
            DescriptionAr = "تقارير مالية واضحة ومنظمة تساعد الأعمال على فهم مركزها المالي بشكل أفضل.",
            IntroAr = "نعد في كور أوديت تقارير مالية واضحة ومنظمة تساعد الأعمال على فهم مركزها المالي بشكل أفضل، ونحرص على أن تكون مرتبة وسهلة الفهم.",
            DetailParagraphsAr = new[]
            {
                "يعتمد إعداد التقارير على المعلومات والسجلات المالية التي يزودنا بها العميل.",
                "يحدد شكل التقارير ومدتها الدورية بالاتفاق معك."
            },
            LeadInAr = "تشمل الخدمة بحسب الاتفاق:",
            BulletsAr = new[]
            {
                "إعداد التقارير والبيانات المالية.",
                "تنظيم المعلومات المالية في عرض واضح.",
                "مراجعة اتساق الأرقام والبيانات المعروضة.",
                "شرح محتوى التقارير عند الحاجة."
            },
            DeliverablesAr = new[]
            {
                "تقارير مالية منظمة بحسب الاتفاق."
            },
            CtaHeadingAr = "هل ترغب في تقارير مالية أوضح؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة احتياجاتك من التقارير المالية."
        },
        new ServiceItem
        {
            Slug = "business-consulting",
            TitleAr = "استشارات الأعمال",
            TitleEn = "Business Consulting",
            Icon = "bi-lightbulb",
            ImagePath = "/images/services/business-consulting.svg",
            DescriptionAr = "رؤى مالية وتجارية تدعم عمليات التخطيط واتخاذ القرار على مستوى الأعمال.",
            IntroAr = "نقدم في كور أوديت رؤى مالية وتجارية تدعم عمليات التخطيط واتخاذ القرار على مستوى الأعمال، انطلاقًا من فهم احتياجاتك وطبيعة نشاطك.",
            DetailParagraphsAr = new[]
            {
                "تبدأ الاستشارة بفهم وضع العمل والأهداف المطروحة.",
                "تعرض الرؤى والتوصيات بصورة واضحة ومنظمة."
            },
            LeadInAr = "يشمل الدعم بحسب نطاق العمل المتفق عليه:",
            BulletsAr = new[]
            {
                "مناقشة الجوانب المالية والتجارية للقرارات المطروحة.",
                "المساعدة في تنظيم عمليات التخطيط.",
                "تحليل المعلومات المتاحة ذات الصلة.",
                "عرض الرؤى والتوصيات بشكل واضح."
            },
            DeliverablesAr = new[]
            {
                "ملاحظات وتوصيات بحسب نطاق الاستشارة."
            },
            CtaHeadingAr = "هل تحتاج إلى رؤى تدعم قراراتك؟",
            CtaTextAr = "تواصل مع فريق كور أوديت لمناقشة احتياجاتك وطلب استشارة."
        },
        new ServiceItem
        {
            Slug = "judicial-support",
            TitleAr = "الدعم القضائي",
            TitleEn = "Judicial Support",
            Icon = "bi-bank2",
            ImagePath = "/images/services/judicial-support.svg",
            DescriptionAr = "دعم محاسبي وفني للقضايا والنزاعات يشمل تقارير الخبرة وحصر الموجودات والمديونيات.",
            HeroSubAr = "دعم محاسبي وفني للقضايا والنزاعات",
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
