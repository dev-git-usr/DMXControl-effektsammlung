using Lumos.GUI.UICustomization;
using LumosLIB.Kernel.Log;
using LumosLIB.Kernel.Scene.Fanning;
using org.dmxc.lumos.Kernel.PropertyType;
using org.dmxc.lumos.Kernel.PropertyValue.Attachable;
using org.dmxc.lumos.Kernel.Scene.Fanning;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.dmxc.lumos.Kernel.PropertyValue.Effect
{
    public class Star : Abstract2DFrequentFunctionEffect
    {
        private static readonly ILumosLog log = LumosLogger.getInstance(typeof(Star));
        private object _corners;
        private PointF[] Pointlist;
        private const double default_Phase = 0.0;
        private const double default_Index = 0.0;
        private const double default_Corner = 4.0;
        private const double default_RotationFrequency = 0.0;
        protected const string INDEXROTATION = "Indexing";
        protected const string PARAMETER_Corners = "Corners";
        protected const string PARAMETER_Amplitude_X = "Amplitude X";
        protected const string PARAMETER_Amplitude_Y = "Amplitude Y";
        protected const string PARAMETER_Index = "Index";
        protected const string PARAMETER_RotationFrequency = "RotationFrequency";

        [UpdateParameter]
        public object Frequency { get; set; }

        [UpdateParameter]
        public object RotationFrequency { get; set; }

        [UpdateParameter]
        public object Phase { get; set; }

        [UpdateParameter]
        public object Index { get; set; }
        [UpdateParameter]
        public object Scale { get; set; }

        [UpdateParameter]
        public object Corners
        {
            get
            {
                return this._corners;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                this._corners = value;
                //Corners ist die Anzahl der äußeren Sternzacken
                int length = (int)FannedValueManager.ToDouble(this.Corners);

                double innerradius = 1.0 * FannedValueManager.ToDouble(this.Scale);
                double outerradius = 1.0;
                //FannedValueManager.ToDouble(this.outer_Radius);
                if (length <= 1)
                    throw new IndexOutOfRangeException();
                // Kombinieren der Punkte
                this.Pointlist = new PointF[2*length];
                for (int index = 0; index < (2*length); index = index + 2)
                {
                    this.Pointlist[index] = new PointF((float)Math.Cos((double)(2 * index) * Math.PI / (double)(2 * length)) * (float)innerradius, (float)Math.Sin((double)(2 * index) * Math.PI / (double)(2 * length)) * (float)innerradius);
                    this.Pointlist[index+1] = new PointF((float)Math.Cos((double)(2 * (index + 1)) * Math.PI / (double)(2 * length)) * (float)outerradius, (float)Math.Sin((double)(2 * (index + 1)) * Math.PI / (double)(2 * length)) * (float)outerradius);

                }
            }
        }

        public Star()
          : this((object)100.0, (object)100.0, (object)0.1, (object)0.0, (object)0.0, (object)0.3, (object)5.0, (object)0.0)
        {
        }

        protected Star(
          object amplitudeX,
          object amplitudeY,
          object frequency,
          object phase,
          object index,
          object scale,
          object corners,
          object rotationFrequency)
          : base(Guid.NewGuid().ToString())
        {
            this.AmplitudeX = amplitudeX;
            this.AmplitudeY = amplitudeY;
            this.Frequency = frequency;
            this.RotationFrequency = rotationFrequency;
            this.Phase = phase;
            this.Index = index;
            this.Scale = scale;
            this.Corners = corners;
        }

        public override string Name
        {
            get
            {
                return nameof(Star);
            }
        }

        protected override ILumosLog Log
        {
            get
            {
                return Star.log;
            }
        }

        protected override double[] getEffectVector2D(long timeInMs)
        {
            double num1 = FannedValueManager.ToDouble(this.Phase);
            double num2 = FannedValueManager.ToDouble(this.RotationFrequency);
            double num3 = this.PeriodicTime * num1 / 360.0;
            if (double.IsInfinity(this.SideDuration))
                throw new NotFiniteNumberException(string.Empty, this.SideDuration);
            if (double.IsInfinity(this.PeriodicTime))
                throw new NotFiniteNumberException(string.Empty, this.PeriodicTime);
            FannedValueManager.ToDouble(this.Corners);
            double[] xy = this.GetXY(((double)timeInMs + num3) % this.PeriodicTime, this.SideDuration);
            double num4 = xy[1];
            double num5 = xy[0];
            double num6 = FannedValueManager.ToDouble(this.Index) * 2.0 * Math.PI / 360.0;
            if (num2 != 0.0)
                num6 += (double)timeInMs / 1000.0 * num2 * 2.0 * Math.PI;
            return new double[2]
            {
        num4 * Math.Cos(num6) - num5 * Math.Sin(num6),
        num5 * Math.Cos(num6) + num4 * Math.Sin(num6)
            };
        }

        protected override IEnumerable<AbstractEffect> getFannedChildren(
          int count)
        {
            for (int i = 0; i < count; ++i)
                yield return (AbstractEffect)Activator.CreateInstance(this.GetType());
        }

        public override sealed double[,] GetPictureData(int dataCount)
        {
            FannedValueManager.ToDouble(this.AmplitudeX);
            FannedValueManager.ToDouble(this.AmplitudeY);
            FannedValueManager.ToDouble(this.Frequency);
            FannedValueManager.ToDouble(this.Phase);
            double num1 = -FannedValueManager.ToDouble(this.Index) * 2.0 * Math.PI / 360.0;
            //int num2 = 2*(int)FannedValueManager.ToDouble(this.Corners);
            int num2 = this.Pointlist.Length;
            double[,] numArray = new double[2, dataCount];
            for (int index = 0; index < dataCount; ++index)
            {
                double[] xy = this.GetXY((double)index, (double)(dataCount / num2));
                numArray[0, index] = xy[0] * Math.Cos(num1) + xy[1] * Math.Sin(num1);
                numArray[1, index] = xy[1] * Math.Cos(num1) - xy[0] * Math.Sin(num1);
            }
            return numArray;
        }

        public double[] GetXY(double time, double sideDuration)
        {
            try
            {
                //int num1 = 2*(int)FannedValueManager.ToDouble(this.Corners);
                int num1 = this.Pointlist.Length;
                double num2 = time / sideDuration % (double)num1;
                double num4 = num2 - (int)num2;
                PointF cornerPoint = this.Pointlist[(int)num2];
                PointF pointF = (int)num2 == num1 - 1 ? this.Pointlist[0] : this.Pointlist[(int)num2 + 1];
                double num5 = (double)pointF.X - (double)cornerPoint.X;
                double num6 = (double)pointF.Y - (double)cornerPoint.Y;
                return new double[2]
                {
          (double) cornerPoint.Y + num6 * num4,
          (double) cornerPoint.X + num5 * num4
                };
            }
            catch (Exception ex)
            {
                Star.log.Error("", ex);
            }
            return new double[2];
        }

        protected override AbstractEffect cloneAbstractEffect()
        {
            return (AbstractEffect)new Star(this.AmplitudeX, this.AmplitudeY, this.Frequency, this.Phase, this.Index, this.Scale, this.Corners, this.RotationFrequency);
        }

        protected override IEnumerable<AttachableParameter> ParametersInternal
        {
            get
            {
                AttachableParameter attachableParameter7 = new AttachableParameter("Scale", (string)null, typeof(NumericFannedValue));
                attachableParameter7.LowerBound = (object)0.0;
                attachableParameter7.UpperBound = (object)1.0;
                attachableParameter7.ValueType = EUiValueType.Degree;
                AttachableParameter attachableParameter1 = new AttachableParameter("Frequency", (string)null, typeof(NumericFannedValue));
                attachableParameter1.LowerBound = (object)0.001;
                attachableParameter1.UpperBound = (object)15.0;
                attachableParameter1.ValueType = EUiValueType.Speed;
                AttachableParameter attachableParameter2 = attachableParameter1;
                attachableParameter2.UpperBoundObject.OnlyFader = true;
                AttachableParameter attachableParameter3 = new AttachableParameter("Jags", (string)null, typeof(NumericFannedValue))
                {
                    LowerBound = (object)2.0,
                    UpperBound = (object)15.0
                };
                attachableParameter3.UpperBoundObject.OnlyFader = true;
                AttachableParameter attachableParameter4 = new AttachableParameter("RotationFrequency", (string)null, typeof(NumericFannedValue))
                {
                    LowerBound = (object)0.0,
                    UpperBound = (object)1.0
                };
                attachableParameter4.UpperBoundObject.OnlyFader = true;
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
                attachableParameterList.Add(attachableParameter7);
                attachableParameterList.Add(attachableParameter2);
                AttachableParameter attachableParameter5 = new AttachableParameter("Phase", "°", typeof(NumericFannedValue));
                attachableParameter5.LowerBound = (object)0.0;
                attachableParameter5.UpperBound = (object)1080.0;
                attachableParameter5.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter5);
                AttachableParameter attachableParameter6 = new AttachableParameter("Indexing", "°", typeof(NumericFannedValue));
                attachableParameter6.LowerBound = (object)0.0;
                attachableParameter6.UpperBound = (object)360.0;
                attachableParameter6.ValueType = EUiValueType.Degree;
                attachableParameterList.Add(attachableParameter6);
                attachableParameterList.Add(attachableParameter3);
                attachableParameterList.Add(attachableParameter4);
                return (IEnumerable<AttachableParameter>)attachableParameterList;
            }
        }

        protected override bool setParameterInternal(string name, object value)
        {
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
                    case "Jags":
                        this.Corners = (object)valueInstance;
                        return true;
                    case "Frequency":
                        this.Frequency = (object)valueInstance;
                        return true;
                    case "Indexing":
                        this.Index = (object)valueInstance;
                        return true;
                    case "Phase":
                        this.Phase = (object)valueInstance;
                        return true;
                    case "RotationFrequency":
                        this.RotationFrequency = (object)valueInstance;
                        return true;
                    case "Scale":
                        this.Scale = (object)valueInstance;
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
                case "Jags":
                    obj = this.Corners;
                    break;
                case "Frequency":
                    obj = this.Frequency;
                    break;
                case "Indexing":
                    obj = this.Index;
                    break;
                case "Phase":
                    obj = this.Phase;
                    break;
                case "RotationFrequency":
                    obj = this.RotationFrequency;
                    break;
                case "Scale":
                    obj = this.Scale;
                    break;
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

        public double SideDuration
        {
            get
            {
                return this.PeriodicTime / (double)this.Pointlist.Length;
            }
        }
    }
}
