using Lumos.GUI.UICustomization;
using LumosLIB.Kernel.Log;
using LumosLIB.Kernel.Scene.Fanning;
using org.dmxc.lumos.Kernel.PropertyType;
using org.dmxc.lumos.Kernel.PropertyValue.Attachable;
using org.dmxc.lumos.Kernel.Scene.Fanning;
using System;
using System.Collections.Generic;
using System.Drawing;

//Todo:
// Q=7 P=12 outlined - Effektvisualizer stürzt ab
namespace org.dmxc.lumos.Kernel.PropertyValue.Effect
{
    public class Starpolygon : Abstract2DFrequentFunctionEffect
    {
        private static readonly ILumosLog log = LumosLogger.getInstance(typeof(Starpolygon));
        private object _p = (object) 6.0;
        private object _q = (object) 2.0;
        private string _movingtype = "Normal";
        private PointF[] Pointlist;
        private double[] Pointdist;
        private double Pointdistsum;
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
        private bool initializing = true;
        private bool constantspeed = true;
        [UpdateParameter]
        public object Frequency { get; set; }

        [UpdateParameter]
        public object RotationFrequency { get; set; }

        [UpdateParameter]
        public object Phase { get; set; }

        [UpdateParameter]
        public object Index { get; set; }

        [UpdateParameter]
        public string Movingtype
        {
            get
            {
                return this._movingtype;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                this._movingtype = value;
                this.calc_Points();
            }
        }
        
        [UpdateParameter]
        public object P {
            get
            {
                return this._p;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                this._p = value;
                this.calc_Points();
                

            }
        }
        [UpdateParameter]
        public object Q {
            get
            {
                return this._q;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException();
                this._q = value;
                this.initializing = false;
                this.calc_Points();
            }
        }

        public int berechneGgt(int _zahl1, int _zahl2)
        {
            int zahl1 = _zahl1;
            int zahl2 = _zahl2;
            //Diese Variable wird bei Wertzuweisungen zwischen den Zahlen benutzt
            int temp = 0;
            //Der Rückgabewert zweier gegebener Zahlen.
            int ggt = 0;//Solange der Modulo der zwei zahlen nicht 0 ist,
                        //werden Zuweisungen entsprechend demEuklidischen Algorithmus ausgeführt.
            while (zahl1 % zahl2 != 0)
            {
                temp = zahl1 % zahl2;
                zahl1 = zahl2;
                zahl2 = temp;
            }

            ggt = zahl2;

            return ggt;

        }

        private void calc_Points()
        {
            if (this.initializing == false)
            {
                int p = (int)FannedValueManager.ToDouble(this.P);
                int q = (int)FannedValueManager.ToDouble(this.Q);
                if (p <= 1)
                    throw new IndexOutOfRangeException();
                if (q <= 1)
                    throw new IndexOutOfRangeException();

                //Calculating outer Points
                PointF[] outerpointlist = new PointF[p];
                double angle = 0.0;
                for (int index = 0; index < p; index = index + 1)
                {
                    outerpointlist[index] = new PointF((float)Math.Cos(angle) * (float)1.0, (float)Math.Sin(angle) * (float)1.0);
                    angle = angle + ((float)2 * Math.PI / (float)p);
                }
                //Calculating inner Points
                PointF[] innerpointlist = new PointF[p];
                angle = (float)Math.PI/(float)p;
                //Spitzenwinkel = 180°-(q*360*)/p
                float mindst;
                if (p-q > q)
                {
                    mindst = q%p;
                } else
                {
                    mindst = Math.Abs(p-q)%p;
                }
                //mindst = q;
                float spitzenwinkel = ((float)Math.PI - (float)((float)2 * Math.PI * (mindst)) / (float)p);
                /*if (spitzenwinkel > (float)Math.PI / 2)
                {
                    spitzenwinkel = (float)Math.PI - spitzenwinkel;
                }*/
                //innenradius = außenradius *(1-cos(0,5*spitzenwinkel)
                float innerradius = 1.0f * (float)Math.Sin((float)0.5 * spitzenwinkel)/(float)Math.Sin(Math.PI-(float)0.5*spitzenwinkel-(Math.PI*(float)2)/((float)2*p));
                for (int index = 0; index < p; index = index + 1)
                {
                    innerpointlist[index] = new PointF((float)Math.Cos(angle) * innerradius, (float)Math.Sin(angle) * innerradius);
                    angle = angle + ((float)2 * Math.PI / (float)p);
                }
                if ((String)this.Movingtype == "Outlines") {
                    //Punkteliste für Außenkontur
                    this.Pointlist = new PointF[2 * p];
                    for (int index = 0; index < 2 * p; index = index + 2)
                    {
                        this.Pointlist[index] = outerpointlist[index / 2];
                        this.Pointlist[index + 1] = innerpointlist[index / 2];
                    }
                } else
                {
                    //Punkteliste für durch die Mitte
                    this.Pointlist = new PointF[0];
                    int id = 0;
                    int ggt = (berechneGgt(p, q));
                    //if (p%q != 0) //zusammenhaengende Sterne
                    if (ggt == 1) //zusammenhaengende Sterne
                    {
                        for (int index = 0; index < p; index = index + 1)
                        {
                            appendPosition(outerpointlist[id]);
                            id = (id + q) % p;
                        }

                    }
                    else
                    {
                        //int counter = (int)(p/q);
                        int counter = 0;
                        int targetcounter = p / ggt;
                        for (int index = 0; index < (p/ggt + 1) * p; index = index + 1)
                        {
                            appendPosition(outerpointlist[id]);
                            if (counter == targetcounter)
                            {
                                // Beginnen der nächsten Form
                                appendPosition(innerpointlist[id]);
                                id = (id + 1) % p;
                                counter = 0;
                            } else
                            {
                                counter++;
                                id = (id + q) % p;
                            }
                        }
                        /*
                        //for (int index = 0; index < (p/q + 1)*p; index = index + 1)
                        for (int index = 0; index < (p/ggt + 1) * p; index = index + 1)
                        //for (int index = 0; index < (p*q)*p; index = index + 1)
                        {
                            appendPosition(outerpointlist[id]);
                            if (index == counter) {
                                // Beginnen der nächsten Form
                                appendPosition(innerpointlist[id]);
                                id = (id + 1) % p;
                                //counter = counter + (int)(p / q) + 1;
                                counter = counter + ggt + 1;

                            }
                            else
                            {
                                // normale vervollständigung
                                id = (id + q) % p;
                            }                
                        }
                        */
                    }
                }
                calcdistances();
            }
        }
        private void appendPosition(PointF pkt)
        {
            PointF[] temp = new PointF[this.Pointlist.Length + 1];
            for (int index = 0; index < this.Pointlist.Length; ++index)
            {
                temp[index] = this.Pointlist[index];
            }
            temp[temp.Length-1] = pkt;
            this.Pointlist = temp;
        }

        private void calcdistances()
        {
            this.Pointdistsum = 0;
            this.Pointdist = new double[this.Pointlist.Length];
            for (int index = 0; index < this.Pointlist.Length; ++index)
            {
                PointF akt = this.Pointlist[index];
                PointF next = this.Pointlist[(index + 1) % this.Pointlist.Length];
                this.Pointdist[index] = this.Pointdistsum + Math.Sqrt(Math.Pow(Math.Abs((double)(akt.X - next.X)), 2) + Math.Pow(Math.Abs((double)(akt.Y - next.Y)), 2));
                this.Pointdistsum = this.Pointdist[index];
            }
        }
        public Starpolygon()
          : this((object)100.0, (object)100.0, (object)0.1, (object)0.0, (object)0.0, "Normal", (object)6.0, (object)2.0, (object)0.0)
        {
        }

        protected Starpolygon(
          object amplitudeX,
          object amplitudeY,
          object frequency,
          object phase,
          object index,
          string movingtype,
          object p,
          object q,
          object rotationFrequency)
          : base(Guid.NewGuid().ToString())
        {
            this.AmplitudeX = amplitudeX;
            this.AmplitudeY = amplitudeY;
            this.Frequency = frequency;
            this.RotationFrequency = rotationFrequency;
            this.Phase = phase;
            this.Index = index;
            this.Movingtype = movingtype;
            this.P = p;
            this.Q = q;
        }

        public override string Name
        {
            get
            {
                return nameof(Starpolygon);
            }
        }

        protected override ILumosLog Log
        {
            get
            {
                return Starpolygon.log;
            }
        }

        protected override double[] getEffectVector2D(long timeInMs)
        {
            double num1 = FannedValueManager.ToDouble(this.Phase);
            double num2 = FannedValueManager.ToDouble(this.RotationFrequency);
            int p = (int)FannedValueManager.ToDouble(this.P);
            int q = (int)FannedValueManager.ToDouble(this.Q);
            if (p % q == 0)
            {
                num2 = num2 / (p / q);
            }
            double num3 = this.PeriodicTime * num1 / 360.0;
            if (double.IsInfinity(this.SideDuration))
                throw new NotFiniteNumberException(string.Empty, this.SideDuration);
            if (double.IsInfinity(this.PeriodicTime))
                throw new NotFiniteNumberException(string.Empty, this.PeriodicTime);
            double[] xy = this.GetXY(((double)timeInMs + num3) % this.PeriodicTime, this.SideDuration, this.constantspeed);
            double num4 = xy[1];
            double num5 = xy[0];
            double num6 = FannedValueManager.ToDouble(this.Index) * 2.0 * Math.PI / 360.0;
            if (num2 != 0.0)
                num6 += (double)timeInMs / 1000.0 * num2 * 2.0 * Math.PI;
            return new double[2]
            {
        num4 * Math.Cos(num6) - num5 * Math.Sin(num6),
        -num5 * Math.Cos(num6) - num4 * Math.Sin(num6)
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
            int p = (int)FannedValueManager.ToDouble(this.P);
            int q = (int)FannedValueManager.ToDouble(this.Q);
            int length = p*q; //Reduzieren der Grafikdarstellung auf relevanten Bereich (nur bei nicht zusammenhängenden Sternen notwendig)
            if (length > this.Pointlist.Length)
            {
                length = this.Pointlist.Length;
            }
            double num1 = -FannedValueManager.ToDouble(this.Index) * 2.0 * Math.PI / 360.0;
            //int num2 = 2*(int)FannedValueManager.ToDouble(this.Corners);
            int num2 = this.Pointlist.Length;
            double[,] numArray = new double[2, dataCount];
            for (int index = 0; index < dataCount; ++index)
            {
                double[] xy = this.GetXY((double)index, (double)dataCount / length, false);
                numArray[0, index] = xy[0] * Math.Cos(num1) - xy[1] * Math.Sin(num1);
                numArray[1, index] = xy[1] * Math.Cos(num1) + xy[0] * Math.Sin(num1);
            }
            return numArray;
        }

        public double[] GetXY(double time, double sideDuration, bool constantspeed)
        {
            int num1;
            double num2 = 0;
            double num4 = 0;
            try
            {
                //int num1 = 2*(int)FannedValueManager.ToDouble(this.Corners);
                if (constantspeed)
                {
                    //calculate Period of time for one time all forms.
                    //The whole effect contains multiple times all forms.
                    int p = (int)FannedValueManager.ToDouble(this.P);
                    int q = (int)FannedValueManager.ToDouble(this.Q);
                    double period = this.PeriodicTime;
                    //constant speed
                    double abstime = (time % period)/period; //calculate abstime (0...1)
                    double absdist = this.Pointdistsum * abstime;
                    num1 = this.Pointlist.Length;
                    int index = 0;
                    while (absdist >= this.Pointdist[(index)%num1])
                    {
                        ++index;
                        if (index == Pointdist.Length)
                        {
                            break;
                        }
                    }
                    double stepabsdist = absdist;
                    if (index != 0)
                    {
                        stepabsdist = stepabsdist - this.Pointdist[index - 1 % num1];
                    }
                    double stepdiffdist = this.Pointdist[index];
                    if (index != 0)
                    {
                        stepdiffdist = stepdiffdist - this.Pointdist[index - 1];
                    }

                    num2 = index + stepabsdist / stepdiffdist;
                    num4 = num2 - (int)num2;
                } else
                {
                    //constant time
                    num1 = this.Pointlist.Length;
                    num2 = time / sideDuration % (double)num1;
                    num4 = num2 - (int)num2;
                }
                
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
                Starpolygon.log.Error("", ex);
            }
            return new double[2];
        }

        protected override AbstractEffect cloneAbstractEffect()
        {
            return (AbstractEffect)new Starpolygon(this.AmplitudeX, this.AmplitudeY, this.Frequency, this.Phase, this.Index, this.Movingtype, this.P, this.Q, this.RotationFrequency);
        }

        protected override IEnumerable<AttachableParameter> ParametersInternal
        {
            get
            {
                AttachableParameter attachableParameter7 = new AttachableParameter("Q", (string)null, typeof(NumericFannedValue));
                attachableParameter7.LowerBound = (object)2.0;
                attachableParameter7.UpperBound = (object)7.0;
                attachableParameter7.ValueType = EUiValueType.Degree;
                AttachableParameter attachableParameter1 = new AttachableParameter("Frequency", (string)null, typeof(NumericFannedValue));
                attachableParameter1.LowerBound = (object)0.001;
                attachableParameter1.UpperBound = (object)15.0;
                attachableParameter1.ValueType = EUiValueType.Speed;
                AttachableParameter attachableParameter2 = attachableParameter1;
                attachableParameter2.UpperBoundObject.OnlyFader = true;
                AttachableParameter attachableParameter3 = new AttachableParameter("P", (string)null, typeof(NumericFannedValue))
                {
                    LowerBound = (object)5.0,
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
                AttachableParameter attachableParameter9 = new AttachableParameter("Movingtype", (string)null, typeof(string), (object[])new string[2]
                {
                  "Normal",
                  "Outlines"
                });
                attachableParameterList.Add(attachableParameter9);
                return (IEnumerable<AttachableParameter>)attachableParameterList;
            }
        }

        protected override bool setParameterInternal(string name, object value)
        {
            if (name.ToLowerInvariant() == "movingtype")
            {
                this.Movingtype = (string)value;
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
                    case "P":
                        this.P = (object)valueInstance;
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
                    case "Q":
                        this.Q = (object)valueInstance;
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
                case "P":
                    obj = this.P;
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
                case "Q":
                    obj = this.Q;
                    break;
                case "Movingtype":
                    return (object)this.Movingtype;
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
                double period;
                int p = (int)FannedValueManager.ToDouble(this.P);
                int q = (int)FannedValueManager.ToDouble(this.Q);
                try
                {
                    period = 1000.0 / FannedValueManager.ToDouble(this.Frequency);
                    if (p%q==0)
                    {
                        period = period * p / q;
                    }
                    return period;
                    
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
