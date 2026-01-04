public static class FeatSlot
{
    public static FeatBase LoadFeat(string featName)
    {
        switch (featName)
        {
            case "铁骨如山":
                return new Feat_IronBody();

            case "重击":
                return new Feat_HeavyStrike();

            case "不屈意志":
                return new Feat_UnyieldingWill();

            case "耐力之墙":
                return new Feat_StaminaWall();

            case "快速反应":
                return new Feat_QuickReflexes();

            case "轻盈步伐":
                return new Feat_Lightfooted();

            case "迅捷打击":
                return new Feat_RapidStrike();  // 添加迅捷打击专长

            case "急速反击":
                return new Feat_QuickCounter();  // 添加急速反击专长

            default:
                return null;
        }
    }
}
