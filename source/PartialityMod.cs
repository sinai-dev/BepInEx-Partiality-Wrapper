using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Partiality.Modloader
{
    /// <summary>
    /// A copy of the Partiality.Modloader.PartialityMod class. Included here so that the Partiality.dll reference is not needed.
    /// </summary>
    public class PartialityMod
    {
        public string ModID = "NULL";
        public string Version = "0000";
        public int loadPriority = 0;
        public string author = "NULL";

        public bool isEnabled;

        internal void BaseInit()
        {
            Init();
        }

        internal void BaseLoad()
        {
            OnLoad();
        }

        public void DisableMod()
        {
            if (!isEnabled)
                return;

            isEnabled = false;
            OnDisable();
        }

        public void EnableMod()
        {
            if (isEnabled)
                return;

            isEnabled = true;
            OnEnable();
        }

        public virtual void Init()
        {
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnEnable()
        {
        }
    }
}