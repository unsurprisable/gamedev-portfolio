using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CustomerNameListSO : ScriptableObject
{
    public string[] list;

    [ContextMenu("Populate Default Names")]
    private void PopulateDefaultNames()
    {
        list = new string[]{
            "Bob", "Rob", "Tob", "Dob", "Sob", "Gob", "Nob", "Lob", "Mob", "Job",
            "Cob", "Hob", "Pob", "Wob", "Zob", "Yob", "Vob", "Qob", "Xob", "Keb",
            "Brob", "Snob", "Klob", "Blob", "Flob", "Glob", "Plob", "Slob", "Scrob",
            "Drob", "Zlob", "Grubob", "Trob", "Glorbob", "Zoblob", "Knobob", "Vrob",
            "Fribob", "Blorob", "Crubob", "Shlob", "Mlob", "Qlob", "Drobob", "Bzob",
            "Frob", "Clob", "Gribob", "Trubob", "Slorb", "Vlob", "Snorb", "Blubob",
            "Jrob", "Trobb", "Zorb", "Clobob", "Splob", "Wrob", "Glubob", "Drobobob",
            "Skob", "Grob", "Krob", "Jlob", "Blorb", "Srob", "Qrob", "Krobob", "Trlob",
            "Mrob", "Vrobob", "Snobob", "Grobb", "Plorb", "Wlob", "Dlob", "Nrob", "Hrob",
            "Splorb", "Clobobob", "Florb", "Zrob", "Brorb", "Crorb", "Frobob"
        };
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
