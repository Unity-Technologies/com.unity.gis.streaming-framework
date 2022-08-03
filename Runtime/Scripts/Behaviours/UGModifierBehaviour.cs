using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Abstract class allowing to <see cref="Instantiate"/> a <see cref="UGModifier"/>.
    /// </summary>
    public abstract class UGModifierBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Create a new <see cref="UGModifier"/> instance representing this <see cref="UGModifierBehaviour"/>.
        /// </summary>
        /// <returns>The newly created instance.</returns>
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
