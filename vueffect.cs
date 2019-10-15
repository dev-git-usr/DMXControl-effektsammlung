using Lumos.GUI.UICustomization;
using LumosLIB.Kernel.Log;
using LumosLIB.Kernel.Scene.Fanning;
using LumosLIB.Tools;
using org.dmxc.lumos.Kernel.DeviceProperties;
using org.dmxc.lumos.Kernel.DeviceProperties.Base;
using org.dmxc.lumos.Kernel.PropertyType;
using org.dmxc.lumos.Kernel.PropertyValue.Attachable;
using org.dmxc.lumos.Kernel.PropertyValue.Fade;
using org.dmxc.lumos.Kernel.Scene.Fanning;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace org.dmxc.lumos.Kernel.PropertyValue.Filter
{
    public class VUFilter : AbstractEffectFilter
    {
        private static readonly ILumosLog log = LumosLogger.getInstance(typeof(VUFilter));
        private object[] enumObjects = new object[0];
        private const string PEAK = "Peak";
        private IPropertyValueFader _fader;

        private VUFilter(
          object peakValue
          )
        {
            this.PeakValue = peakValue;
        }

        public VUFilter()
          : this((object)null)
        {
        }

        [UpdateParameter(true, false)]
        public object PeakValue { get; set; }

      
        public override string Name
        {
            get
            {
                return "VU-Effekt";
            }
        }

        protected override ILumosLog Log
        {
            get
            {
                return VUFilter.log;
            }
        }

        protected override void SetParameterBoundariesFromUsingContext()
        {
            base.SetParameterBoundariesFromUsingContext();
            if (this.UsingPropertyContext.HasEnumValues)
            {
                this.enumObjects = (object[])((IEnumerable<IEnumObject>)this.UsingPropertyContext.EnumValues).NullToEmpty<IEnumObject>().ToArray<IEnumObject>();
            }
            else
            {
                LumosBoolean lumosBoolean = this.UsingPropertyContext.Value as LumosBoolean;
                if (!(lumosBoolean != (LumosBoolean)null))
                    return;
                this.enumObjects = new object[2]
                {
          (object) new LumosBoolean(true, lumosBoolean.TrueText, lumosBoolean.FalseText),
          (object) new LumosBoolean(false, lumosBoolean.TrueText, lumosBoolean.FalseText)
                };
            }
        }

        protected override void SetInitialValuesFromUsingContext()
        {
            base.SetInitialValuesFromUsingContext();
            if (this.UsingPropertyContext.HasEnumValues)
            {
                if (this.enumObjects.Length != 0)
                    this.setUsingTypeParameter("Peak", this.enumObjects[this.enumObjects.Length - 1]);
                else
                    this.setUsingTypeParameter("Peak", this.UsingPropertyContext.Value);
            }
            else if (this.UsingPropertyContext.HasFilterBounds)
                this.setUsingTypeParameter("Peak", this.UsingPropertyContext.UpperBound);
            else
                this.setUsingTypeParameter("Peak", this.UsingPropertyContext.Value);
        }

        public override bool Mergeable
        {
            get
            {
                return true;
            }
        }

        public override bool canFilter(AttachableParameterBag bag, IDevicePropertyBase prop)
        {
            if (!(prop is IDeviceProperty))
                return false;
            if (bag == null || !bag.Parameters.Contains("Peak"))
                return true;
            object parameter = bag.GetParameter("Peak");
            Type type1 = parameter.GetType();
            Type type2 = ((IDeviceProperty)prop).getValueInstance().Value.GetType();
            if (parameter is IFannedValueContainer)
                type1 = ((IFannedValueContainer)parameter).FanningValuesType;
            return type1 == type2;
        }

        protected override void DeltaReset()
        {
            base.DeltaReset();
            this.UsingContext?.SetSharedValue<object>("Map", (object)null);
        }

        protected override IPropertyValue doFilter(
          IPropertyValue input,
          long timeInMs,
          long delta)
        {
            if (this.PeakValue != null)
            {
                IPropertyValue propertyValue = input.baseClone();
                propertyValue.Value = this.PeakValue;
                return propertyValue;
            }
            return input;
        }

      
        protected override AbstractFilter cloneAbstractFilter()
        {
            return (AbstractFilter)new VUFilter(this.PeakValue);
        }

        protected override IEnumerable<AttachableParameter> ParametersInternal
        {
            get
            {
                if (this.UsingPropertyContext != null && this.UsingPropertyContext.Value != null)
                {
                    AttachableParameter attachableParameter = new AttachableParameter("Peak", (string)null, typeof(NumericFannedValue));
                    if (this.UsingPropertyContext.HasFilterBounds)
                    {
                        attachableParameter.LowerBound = this.UsingPropertyContext.LowerBound;
                        attachableParameter.UpperBound = this.UsingPropertyContext.UpperBound;
                    }
                    yield return attachableParameter;
                }
            }
        }

        protected override object getParameterInternal(string name)
        {
            object obj = (object)null;
            string lowerInvariant = name.ToLowerInvariant();
            if (!(lowerInvariant == "peak"))
            {
          
            }
            else
            {
                if (!FannedValueManager.getInstance().canConvertToContainerInstance(this.PeakValue))
                    return this.PeakValue;
                obj = this.PeakValue;
            }
            return (object)FannedValueManager.getInstance().convertToContainerInstance(obj, EUiValueType.Unknown);
        }

        protected override void setParameterOnLoad(string name, object value)
        {
            if (name.ToLowerInvariant() == "peak")
                this.setParameterInternal(name, value);
            else
                base.setParameterOnLoad(name, value);
        }

        protected override bool setParameterInternal(string name, object value)
        {
            if (FannedValueManager.getInstance().canConvertToValueInstance(value))
            {
                IFannedValue valueInstance = FannedValueManager.getInstance().convertToValueInstance(value);
                if (!(name.ToLowerInvariant() == "peak"))
                {

                }
                else
                {
                    this.PeakValue = (object)valueInstance;
                    return true;
                }
            }
            return base.setParameterInternal(name, value);
        }

        public override bool SupportsAnimation
        {
            get
            {
                return false;
            }
        }

        protected override Bitmap getPicture(int iconSize, long timeInMs)
        {
            int num3 = iconSize;
            Bitmap bitmap = new Bitmap(num3, num3, PixelFormat.Format24bppRgb);
            using (Graphics graphics1 = Graphics.FromImage((Image)bitmap))
            {
                graphics1.Clear(Color.Black);
                GraphicsPath path = new GraphicsPath();
                graphics1.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int)(num3 * 0.1), (int)(num3 * 0.3), (int)(num3 * 0.3), (int)(num3 * 0.6)));
                graphics1.FillRectangle(new SolidBrush(Color.Green), new Rectangle((int)(num3 * 0.6), (int)(num3 * 0.5), (int)(num3 * 0.3), (int)(num3 * 0.4)));
            }
            return bitmap;
        }
    }
}