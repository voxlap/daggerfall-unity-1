// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2021 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects
{
    public class YellowFever : DiseaseEffect
    {
        Diseases diseaseType = Diseases.YellowFever;

        public override void SetProperties()
        {
            properties.Key = GetClassicDiseaseEffectKey(diseaseType);
            properties.ShowSpellIcon = false;
            classicDiseaseType = diseaseType;
            diseaseData = GetClassicDiseaseData(diseaseType);
        }

        public override TextFile.Token[] ContractedMessageTokens => GetClassicContractedMessageTokens(diseaseType);
    }
}
