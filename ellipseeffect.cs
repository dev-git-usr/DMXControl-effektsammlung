using Lumos.GUI.UICustomization;
using LumosLIB.Kernel.Log;
using LumosLIB.Kernel.Scene.Fanning;
using org.dmxc.lumos.Kernel.PropertyType;
using org.dmxc.lumos.Kernel.PropertyValue.Attachable;
using org.dmxc.lumos.Kernel.Scene.Fanning;
using System;
using System.Collections.Generic;

namespace org.dmxc.lumos.Kernel.PropertyValue.Effect
{
    public class Ellipse : Abstract2DFrequentFunctionEffect
    {
        private static readonly ILumosLog log = LumosLogger.getInstance(typeof(Ellipse));
        private const double default_Phase = 0.0;

        public Ellipse()
          : this((object)100.0, (object)100.0, (object)0.1, (object)0.0, (object)0.0, (object)0.0,(object)1.0, (object)0.5, (object)0.0, (object)360.0, "normal")
        {
        }

        protected Ellipse(object amplitudeX, object amplitudeY, object frequency, object phase, object index, object rotationfrequency, object a, object b, object startangle, object stopangle, string playmode)
          : base(Guid.NewGuid().ToString())
        {
            this.AmplitudeX = amplitudeX;
            this.AmplitudeY = amplitudeY;
            this.Frequency = frequency;
            this.Phase = phase;
            this.Index = index;
            this.Rotationfrequency = rotationfrequency;
            this.Parameter_a = a;
            this.Parameter_b = b;
            this.Startangle = startangle;
            this.Stopangle = stopangle;
            this.Playmode = playmode;
        }

        public override string Name
        {
            get
            {
                return "Ellipse";
            }
        }

        protected override ILumosLog Log
        {
            get
            {
                return Ellipse.log;
            }
        }

        [UpdateParameter]
        public object Frequency
        {
            get
            {
                return this.FrequencyX;
            }
            set
            {
                this.FrequencyX = value;
                this.FrequencyY = value;
            }
        }

        [UpdateParameter]
        public object Phase
        {
            get
            {
                return this.PhaseX;
            }
            set
            {
                this.PhaseX = value;
                this.PhaseY = value;
            }
        }

        [UpdateParameter]
        public object Index { get; set; }

        [UpdateParameter]
        public object Rotationfrequency { get; set; }

        [UpdateParameter]
        public object Parameter_a { get; set; }

        [UpdateParameter]
        public object Parameter_b { get; set; }

        [UpdateParameter]
        public object Startangle { get; set; }
        
        [UpdateParameter]
        public object Stopangle { get; set; }

        [UpdateParameter]
        public string Playmode { get; set; }

        public override sealed double[,] GetPictureData(int dataCount)
        {
            double indexing = - FannedValueManager.ToDouble(this.Index) * 2.0 * Math.PI / 360.0;
            double[,] numArray = new double[2, dataCount];
            for (int index = 0; index < dataCount; ++index)
            {
                double[] xy = this.GetXY((double)index, (double)dataCount);
                numArray[0, index] = xy[0] * Math.Cos(indexing) - xy[1] * Math.Sin(indexing);
                numArray[1, index] = xy[1] * Math.Cos(indexing) + xy[0] * Math.Sin(indexing);
            }
            return numArray;
        }

        protected override double[] getEffectVector2D(long timeInMs)
        {
            double num = this.PeriodicTime * FannedValueManager.ToDouble(this.Phase) / 360.0;
            double num2 = FannedValueManager.ToDouble(this.Rotationfrequency);
            double num6 = FannedValueManager.ToDouble(this.Index) * 2.0 * Math.PI / 360.0;
            double[] xy = this.GetXY(((double)timeInMs + num) % this.PeriodicTime, this.PeriodicTime);

            if (num2 != 0.0) //Rotate if necessary
                num6 += (double)timeInMs / 1000.0 * num2 * 2.0 * Math.PI;
            return new double[2]
            {
                xy[1] * Math.Cos(num6) - xy[0] * Math.Sin(num6),
                - xy[0] * Math.Cos(num6) - xy[1] * Math.Sin(num6)
            };
        }

        public double[] GetXY(double time, double periodTime)
        {
            double a = FannedValueManager.ToDouble(this.Parameter_a);
            double b = FannedValueManager.ToDouble(this.Parameter_b);
            double start = FannedValueManager.ToDouble(this.Startangle) * 2.0 * Math.PI / 360;
            double stop = FannedValueManager.ToDouble(this.Stopangle) * 2.0 * Math.PI / 360;
            double num1 = 0.0;
            double num2 = 0.0;
            double deltaangle = stop - start;
            double angle = 0;
            if (this.Playmode == "normal")
            {
                //normal mode
                angle = start + (deltaangle * time / periodTime);
            } else
            {
                //bounce mode
                if ((time/periodTime)%1 > 0.5f)
                { //Rücklaufend
                    angle = start + (deltaangle * ((2.0 * time / periodTime)%1));
                } else
                { //Vorlaufend
                    angle = stop - (deltaangle * ((2.0 * time / periodTime)%1));
                }
                
            }
            try
            {
                num1 = Math.Cos(angle) * a;
                num2 = Math.Sin(angle) * b;
            }
            catch (Exception ex)
            {
                Ellipse.log.Error("", ex);
            }
            return new double[2] { num2, num1 };
        }

        protected override IEnumerable<AbstractEffect> getFannedChildren(
          int count)
        {
            for (int i = 0; i < count; ++i)
                yield return (AbstractEffect)Activator.CreateInstance(this.GetType());
        }

        protected override IEnumerable<AttachableParameter> ParametersInternal
        {
            get
            {
                AttachableParameter attachableParameter1 = new AttachableParameter("Frequency", (string)null, typeof(NumericFannedValue));
                attachableParameter1.LowerBound = (object)0.0;
                attachableParameter1.UpperBound = (object)15.0;
                attachableParameter1.ValueType = EUiValueType.Speed;
                AttachableParameter attachableParameter2 = attachableParameter1;
                attachableParameter2.UpperBoundObject.OnlyFader = true;
                Position position1 = new Position(-90.0, -90.0);
                Position position2 = new Position(90.0, 90.0);
                if (this.UsingPropertyContext != null && this.UsingPropertyContext.ValueType == typeof(Position) && this.UsingPropertyContext.HasFilterBounds)
                {
                    position1 = (Position)this.UsingPropertyContext.LowerBound;
                    position2 = (Position)this.UsingPropertyContext.UpperBound;
                }
                List<AttachableParameter> attachableParameterList = new List<AttachableParameter>();
                attachableParameterList.Add(new AttachableParameter("Amplitude X", (string)null, typeof(NumericFannedValue))
                {
                    LowerBound = (object)position1.Pan,
                    UpperBound = (object)position2.Pan
                });
                attachableParameterList.Add(new AttachableParameter("Amplitude Y", (string)null, typeof(NumericFannedValue))
                {
                    LowerBound = (object)position1.Tilt,
                    UpperBound = (object)position2.Tilt
                });
                attachableParameterList.Add(attachableParameter2);
                AttachableParameter attachableParameter3 = new AttachableParameter("Phase", "°", typeof(NumericFannedValue));
                attachableParameter3.LowerBound = (object)0.0;
                attachableParameter3.UpperBound = (object)1080.0;
                attachableParameter3.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter3);
                AttachableParameter attachableParameter4 = new AttachableParameter("Indexing", "°", typeof(NumericFannedValue));
                attachableParameter4.LowerBound = (object)0.0;
                attachableParameter4.UpperBound = (object)1080.0;
                attachableParameter4.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter4);
                AttachableParameter attachableParameter5 = new AttachableParameter("Rotationfrequency", null, typeof(NumericFannedValue));
                attachableParameter5.LowerBound = (object)0.0;
                attachableParameter5.UpperBound = (object)1.0;
                attachableParameter5.ValueType = EUiValueType.Degree;
                attachableParameter5.UpperBoundObject.OnlyFader = true;
                attachableParameterList.Add(attachableParameter5);
                AttachableParameter attachableParameter6 = new AttachableParameter("a", null, typeof(NumericFannedValue));
                attachableParameter6.LowerBound = (object)0.0;
                attachableParameter6.UpperBound = (object)1.0;
                attachableParameter6.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter6);
                AttachableParameter attachableParameter7 = new AttachableParameter("b", null, typeof(NumericFannedValue));
                attachableParameter7.LowerBound = (object)0.0;
                attachableParameter7.UpperBound = (object)1.0;
                attachableParameter7.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter7);
                AttachableParameter attachableParameter8 = new AttachableParameter("Startangle", null, typeof(NumericFannedValue));
                attachableParameter8.LowerBound = (object)0.0;
                attachableParameter8.UpperBound = (object)360.0;
                attachableParameter8.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter8);
                AttachableParameter attachableParameter9 = new AttachableParameter("Stopangle", null, typeof(NumericFannedValue));
                attachableParameter9.LowerBound = (object)0.0;
                attachableParameter9.UpperBound = (object)360.0;
                attachableParameter9.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter9);
                AttachableParameter attachableParameter10 = new AttachableParameter("Playmode", (string)null, typeof(string), (object[])new string[2]
               {
                  "normal",
                  "bounce"
               });
                attachableParameterList.Add(attachableParameter10);
                return (IEnumerable<AttachableParameter>)attachableParameterList;
            }
        }

        protected override bool setParameterInternal(string name, object value)
        {
            if (name.ToLowerInvariant() == "playmode")
            {
                this.Playmode = (string)value;
                return true;
            }
            if (FannedValueManager.getInstance().canConvertToValueInstance(value))
            {
                IFannedValue valueInstance = FannedValueManager.getInstance().convertToValueInstance(value);
                switch (name)
                {
                    case "Amplitude X":
                        this.AmplitudeX = (object)valueInstance;
                        return true;
                    case "Amplitude Y":
                        this.AmplitudeY = (object)valueInstance;
                        return true;
                    case "Frequency":
                        this.Frequency = (object)valueInstance;
                        return true;
                    case "Phase":
                        this.Phase = (object)valueInstance;
                        return true;
                    case "Indexing":
                        this.Index = (object)valueInstance;
                        return true;
                    case "Rotationfrequency":
                        this.Rotationfrequency = (object)valueInstance;
                        return true;
                    case "a":
                        this.Parameter_a = (object)valueInstance;
                        return true;
                    case "b":
                        this.Parameter_b = (object)valueInstance;
                        return true;
                    case "Startangle":
                        this.Startangle = (object)valueInstance;
                        return true;
                    case "Stopangle":
                        this.Stopangle = (object)valueInstance;
                        return true;
                }
            }
            return base.setParameterInternal(name, value);
        }

        protected override object getParameterInternal(string name)
        {
            object obj = (object)null;
            switch (name)
            {
                case "Amplitude X":
                    obj = this.AmplitudeX;
                    break;
                case "Amplitude Y":
                    obj = this.AmplitudeY;
                    break;
                case "Frequency":
                    obj = this.Frequency;
                    break;
                case "Phase":
                    obj = this.Phase;
                    break;
                case "Indexing":
                    obj = this.Index;
                    break;
                case "Rotationfrequency":
                    obj = this.Rotationfrequency;
                    break;
                case "a":
                    obj = this.Parameter_a;
                    break;
                case "b":
                    obj = this.Parameter_b;
                    break;
                case "Startangle":
                    obj = this.Startangle;
                    break;
                case "Stopangle":
                    obj = this.Stopangle;
                    break;
                case "Playmode":
                    return (object)this.Playmode;
            }
            return (object)FannedValueManager.getInstance().convertToContainerInstance(obj, EUiValueType.Unknown);
        }

        protected override void SetInitialValuesFromUsingContext()
        {
            base.SetInitialValuesFromUsingContext();
            if (!(this.UsingPropertyContext.ValueType == typeof(Position)) || !this.UsingPropertyContext.HasFilterBounds)
                return;
            Position upperBound = (Position)this.UsingPropertyContext.UpperBound;
            Position lowerBound = (Position)this.UsingPropertyContext.LowerBound;
            this.setUsingTypeParameter("Amplitude X", (object)((upperBound.Pan - lowerBound.Pan) / 2.0));
            this.setUsingTypeParameter("Amplitude Y", (object)((upperBound.Tilt - lowerBound.Tilt) / 2.0));
        }

        protected override AbstractEffect cloneAbstractEffect()
        {
            return (AbstractEffect)new Ellipse(this.AmplitudeX, this.AmplitudeY, this.Frequency, this.Phase, this.Index, this.Rotationfrequency,this.Parameter_a, this.Parameter_b, this.Startangle, this.Stopangle,this.Playmode);
        }

        public override double PeriodicTime
        {
            get
            {
                try
                {
                    return 1000.0 / FannedValueManager.ToDouble(this.Frequency);
                }
                catch
                {
                    return 0.0;
                }
            }
        }
    }
}
