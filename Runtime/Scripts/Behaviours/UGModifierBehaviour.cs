using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public abstract class UGModifierBehaviour : MonoBehaviour
    {
        public abstract UGModifier Instantiate();

        /// <summary>
        /// Modifier validation method against a given UGSystem
        /// </summary>
        /// <param name="system">
        /// A UGSystem that currently uses this UGModifier
        /// </param>
        /// <param name="errorMsg">
        /// When this method returns, outputs a validation error message if there is any error, otherwise outputs an empty string
        /// </param>
        /// <returns>Return false if there is any validation error, otherwise return true</returns>
        public virtual bool Validate(UGSystemBehaviour system, out string errorMsg) 
        {
            errorMsg = string.Empty;
            return true; 
        }
    }

}
