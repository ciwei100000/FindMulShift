/*
  * Created by SharpDevelop.
  * Program:        FindMulShift demo
  * Description:	Find a  " * mul >> shift " equivalent for dividing
  * 					with an integer for a certain integer range
  * 
  * inspired by Jeffrey Sax's shift multiply optimization
  * http://geekswithblogs.net/akraus1/archive/2006/04/23/76146.aspx
  * 
  * version:     1.0
  *
  * User:        rob tillaart
  * Date:        08/09/2006
  * Time:        16:58
  *
  * Notes 
  * - Negative numbers and negative dividers should strip of their sign bit first
  *     and divide in the positive range, see sample code 
  * - Valid values for shift and mul can be calculated directly, but these are 
  * 	not allways optimal and cannot allways be reduced to the optimal value.
  *     this reduces the working range
  * - mul and shift are longs, note however the optimization works especially for int
  * 
  */
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Test
{
	
    class MainClass
    {
        public static void Main(string[] args)
        {
        	// demo();
        	
        	Console.Write("Denominator: ");
        	long div = long.Parse(Console.ReadLine());
        	Console.Write("Max enumerator: ");
        	long max = long.Parse(Console.ReadLine());
        	long mul;
        	int shift;
        	
        	if (Optimize.FindMulShift(max, div, out mul, out shift))
            {
                string msg = " n / " +
                        div.ToString() + "\t <==> n * " + mul.ToString() + " >> " + shift.ToString();
                System.Console.WriteLine(msg);
            }
            else
            {
                System.Console.WriteLine(div.ToString() + " not found");
            }
            System.Console.ReadLine();

        }
        
        public static void demo()
        {
            long mul = 0;
            int shift = 0;
            long div = 0;
            long max = 0;
 
            int high = 1000;
            int maxdiv = 100;
            int iterations = 1000;
            
            // some doubles to average timing
            double avg = 0;
            double s1;
            double s2;
            
            HiPerfTimer pt = new HiPerfTimer();
          
            #region example I
            
            pt.Start();
            for ( div = 1; div < 100 ; div++)
            {
            	max = 1000; 	// 1L<<30; // = 2^30
                if (Optimize.FindMulShift(max, div, out mul, out shift))
                {
                    string msg = " / " +
                        div.ToString() + "\t <==>  * " + mul.ToString() + " >> " + shift.ToString();
                    System.Console.WriteLine(msg);
                }
                else
                {
                    System.Console.WriteLine(div.ToString() + " not found");
                    System.Console.ReadLine();
                }
            }
            pt.Stop();
            
            System.Console.WriteLine("done, " + pt.Duration().ToString("0.00") );
            System.Console.ReadLine();
                    
            #endregion
            	            
			#region performance measurement / 
            //
			// How much faster is it?
            //
            avg = 0;
            
            System.Console.WriteLine("start / ");            
            for( div = -maxdiv; div <= maxdiv; div++)
            {
            	if (div==0) continue;
            	
            	Optimize.FindMulShift(high, div, out mul, out shift);
	            
	            // normal div timing,
	            pt.Start();
	            long x=0;
	            for (int i=0; i<iterations; i++)
	            {
	            	for (int j=-high; j<high; j++)
	            	{
	            		x = j / div;
	        		}
	        	}
	            pt.Stop();
	            s1 = pt.Duration();
	            
	            // mul shift timing, 
	            // with fixed dividers, the if then else construct will be simpler!!
	            // just as example how to handle negative dividers
	            pt.Start();
	            for (int i=0; i<iterations; i++)
	            {
	            	for (int j=-high; j<high; j++)
	            	{
	            		if (j >= 0) 
	            		{
	            			if (div > 0) 
	            			{
	            				x = ((j * mul) >> shift);
	            			} 
	            			else
	            			{
	            				x = -((j * mul) >> shift);
	            			}
	            		}
	            		else
	            		{
	            			if (div > 0) 
	            			{
	            				x = -((-j * mul) >> shift);
	            			} 
	            			else
	            			{
	            				x = ((-j * mul) >> shift);
	            			}
	            		}
	            	}
	        	}
	           	pt.Stop();
	           	s2 = pt.Duration();
	            
	            double d = 100 - 100 * s2/s1; 

	            System.Console.WriteLine(div + "\t" 
	                                     + s1.ToString("0.000")
	                                     + "\t" + s2.ToString("0.000") 
	                                     + "\tperc: " + d.ToString("0.00"));
	            avg += d;
            }
            // TODO: remove hard coded average 
            System.Console.WriteLine("average: " + (avg/maxdiv/2).ToString("0.00"));
            
            System.Console.WriteLine("done");
            System.Console.ReadLine();
            
			#endregion

            #region performance measurement %
            //
			// How much faster is it?
            //
            avg = 0;
            
            System.Console.WriteLine("start %");            
            for(div = -maxdiv; div <= maxdiv; div++)
            {
            	if (div==0) continue;
            	
            	Optimize.FindMulShift(high, div, out mul, out shift);
	            
	            // normal div timing,
	            pt.Start();
	            long x=0;
	            for (int i=0; i<iterations; i++)
	            {
	            	for (int j=-high; j<high; j++)
	            	{
	            		x = j % div;
	        		}
	        	}
	            pt.Stop();
	            s1 = pt.Duration();
	            
	            // mul shift timing, 
	            // with fixed dividers, the if then else construct will be simpler!!
	            pt.Start();
	            for (int i=0; i<iterations; i++)
	            {
	            	//long y = 0;
	            	for (int j=-high; j<high; j++)
	            	{
	            		if (j >= 0) 
	            		{
	            			if (div > 0) 
	            			{
	            				x = j - ((j * mul) >> shift) * div;
	            			} 
	            			else
	            			{
	            				x = j - ((j * mul) >> shift) * -div;
	            			}
	            		}
	            		else
	            		{
	            			if (div > 0) 
	            			{
	            				x = j + ((-j * mul) >> shift) * div;
	            			} 
	            			else
	            			{
	            				x = j + ((-j * mul) >> shift) * (-div);
	            			}
	            		}
//	            		if (x != j%div)
//	            		{
//	            			Console.WriteLine(j + " " + div + " "+ x + " "  + y);
//	            		}
	            	}
	        	}
	           	pt.Stop();
	           	s2 = pt.Duration();
	            
	            double d = 100 - 100 * s2/s1; 

	            System.Console.WriteLine(div + "\t" 
	                                     + s1.ToString("0.000")
	                                     + "\t" + s2.ToString("0.000") 
	                                     + "\tperc: " + d.ToString("0.00"));
	            avg += d;
            }
            System.Console.WriteLine("average: " + (avg/maxdiv/2).ToString("0.00"));
            
            System.Console.WriteLine("done");
            System.Console.ReadLine();
            
            #endregion

            #region performance measurement / (positive only)
            //
			// How much faster is it?
            //
            avg = 0;
            
            System.Console.WriteLine("start / (only positive values)");            
            for(div = 1; div <= maxdiv; div++)
            {
            	if (div==0) continue;		// allows div to start with -maxdiv
            	
            	Optimize.FindMulShift(high, div, out mul, out shift);
	            
	            // normal div timing,
	            pt.Start();
	            long x=0;
	            for (int i=0; i<iterations; i++)
	            {
	            	for (int j=0; j<high; j++)
	            	{
	            		x = j / div;
	        		}
	        	}
	            pt.Stop();
	            s1 = pt.Duration();
	            
	            // mul shift timing, 
	            // with fixed dividers, the if then else construct will be simpler!!
	            pt.Start();
	            for (int i=0; i<iterations; i++)
	            {
	            	for (int j=0; j<high; j++)
	            	{
	            		x = ((j * mul) >> shift);
	            	}
	        	}
	           	pt.Stop();
	           	s2 = pt.Duration();
	            
	           	// calculate gain 
	            double d = 100 - 100 * s2/s1; 

	            System.Console.WriteLine(div + "\t" 
	                                     + s1.ToString("0.000")
	                                     + "\t" + s2.ToString("0.000") 
	                                     + "\tperc: " + d.ToString("0.00"));
	            avg += d;
            }
            System.Console.WriteLine("average: " + (avg/maxdiv).ToString("0.00"));
            
            System.Console.WriteLine("done");
            System.Console.ReadLine();
            
			#endregion
 
        } // Main

    } 
    
    #region Optimize
    
    class Optimize
    {
        /*
         <summary>
         FindMulShift finds the multiply and shift to replace a division for a range of
         positive integers defined by min and max.
          
           a / n == a * mul >> shift
         
         As we know 1/n = 1 * mul >> shift, we can solve mul = (1 << shift) / n
         
         </summary>
         <param name="max">positive integer, upper limit</param>
         <param name="div">divider to replace</param>
         <param name="mul">multiply factor</param>
         <param name="shift">shift factor (0..63)</param>
         <returns>
         True if a valid mul and shift value are found. 
         If invald both mul and shift are -1
         </returns>
        */
        public static bool FindMulShift(long max, long div, out long mul, out int shift)
        {
        	max = Math.Abs(max);
        	div = Math.Abs(div);
        	
            bool found = false;
            mul = -1;
            shift = -1;

            // zero divider error
            if (div == 0) return false;
            
  			// this division would always return 0 from 0..max
  			if (max < div) 
  			{
  				mul = 0;
  				shift = 0;
  				return true;
  			}

            // catch powers of 2
            for (int s = 0; s <= 63; s++)
            {
                if (div == (1L << s))
                {
                    mul = 1;
                    shift = s;
                    return true;
                }
            }

            // start searching for a valid mul/shift pair
            for (shift = 1; shift <= 62; shift++)
            {
            	// shift factor is at least 2log(div), skip others
            	if ((1L << shift) <= div) continue;
            	
                // we calculate a candidate for mul
                mul = (1L << shift) / div + 1;

                // assume it is a good one
                found = true;

                // test if it works for the range 0 .. max
                // Note: takes too much time for large values of max. 
                if (max < 1000000)
                {
	                for (long i = max; i >=1; i--)		// testing large values first fails faster 
	                {
	                	if ((i / div) != ((i * mul) >> shift))
	                    {
	                        found = false;
	                        break;
	                    }
	                }
                }
                else
                {
					// very fast test, no mathematical proof yet but it seems to work well
					// test highest number-pair for which the division must 'jump' correctly
					// test max, to be sure;
					long t = (max/div +1) * div;
					if ((((t-1) / div) != (((t-1) * mul) >> shift)) ||
					    ((t / div) != ((t * mul) >> shift)) ||
					    ((max / div) != ((max * mul) >> shift))
					   )
	                {
	                	found = false;
	                }
                }

                // are we ready?
                if (found)
                {
                	break;
                }
            }
            return found;
        }

    }
    
    #endregion
    
    #region HiPerfTimer
    /// <summary>
    /// Thanx to Daniel Strigl
    /// http://www.codeproject.com/csharp/highperformancetimercshar.asp
    /// slightly modified ..
    /// </summary>
    class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long startTime, stopTime, splitTime;
        private long freq;
        private double mul;

        // Constructor
        public HiPerfTimer()
        {
            startTime = 0;
            stopTime  = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
            mul = 1.0/freq;			// multiply is cheaper than division
        }

        // Start the timer
        public void Start()
        {
            // lets do the waiting threads there work
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }

        // Returns split time of the timer in seconds.
        public double Split()
		{
			QueryPerformanceCounter( out splitTime );
			return (splitTime - startTime ) * mul; 
		}
        
        // Stop the timer
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        // Returns the duration of the timer (in seconds)
        public double Duration()
        {
	        return (stopTime - startTime) * mul;
        }
    }
    #endregion
}


