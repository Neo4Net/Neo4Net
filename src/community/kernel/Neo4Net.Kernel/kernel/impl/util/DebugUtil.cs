using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.util
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.nanoTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Exceptions.stringify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.duration;

	public class DebugUtil
	{
		 private DebugUtil()
		 {
		 }

		 public static void LogTrace( string fmt, params object[] args )
		 {
			  LogTrace( 2, 5, fmt, args );
		 }

		 public static void LogTrace( int skip, int limit, string fmt, params object[] args )
		 {
			  if ( EnabledAssertions() )
			  {
					Thread thread = Thread.CurrentThread;
					string threadName = thread.Name;
					ThreadGroup group = thread.ThreadGroup;
					string groupPart = group != null ? " in group " + group.Name : "";
					string message = "[" + threadName + groupPart + "] " + string.format( fmt, args );
					TraceLog traceLog = new TraceLog( message );
					PrintLimitedStackTrace( System.err, traceLog, skip, limit );
			  }
		 }

		 private static void PrintLimitedStackTrace( PrintStream @out, Exception cause, int skip, int limit )
		 {
			  lock ( @out )
			  {
					string[] lines = stringify( cause ).Split( lineSeparator() );
					foreach ( string line in lines )
					{
						 if ( line.StartsWith( "\tat ", StringComparison.Ordinal ) )
						 {
							  if ( skip > 0 )
							  {
									skip--;
							  }
							  else if ( limit > 0 )
							  {
									limit--;
									@out.println( line );
							  }
							  else
							  {
									break;
							  }
						 }
						 else
						 {
							  @out.println( line );
						 }
					}
			  }
		 }

		 public static void PrintShortStackTrace( Exception cause, int maxNumberOfStackLines )
		 {
			  Console.WriteLine( FirstLinesOf( stringify( cause ), maxNumberOfStackLines + 1 ) );
		 }

		 public static string FirstLinesOf( string @string, int maxNumberOfLines )
		 {
			  // Totally verbose implementation of this functionality :)
			  StringWriter stringWriter = new StringWriter();
			  PrintWriter writer = new PrintWriter( stringWriter );
			  try
			  {
					using ( StreamReader reader = new StreamReader( new StringReader( @string ) ) )
					{
						 string line = null;
						 for ( int count = 0; ( line = reader.ReadLine() ) != null && count < maxNumberOfLines; count++ )
						 {
							  writer.println( line );
						 }
					}
					writer.close();
					return stringWriter.Buffer.ToString();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Can't happen", e );
			  }
		 }

		 public static bool StackTraceContains( Thread thread, System.Predicate<StackTraceElement> predicate )
		 {
			  foreach ( StackTraceElement element in thread.StackTrace )
			  {
					if ( predicate( element ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public static bool CurrentStackTraceContains( System.Predicate<StackTraceElement> predicate )
		 {
			  return StackTraceContains( Thread.CurrentThread, predicate );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<StackTraceElement> classNameIs(final String className)
		 public static System.Predicate<StackTraceElement> ClassNameIs( string className )
		 {
			  return item => item.ClassName.Equals( className );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<StackTraceElement> classNameContains(final String classNamePart)
		 public static System.Predicate<StackTraceElement> ClassNameContains( string classNamePart )
		 {
			  return item => item.ClassName.contains( classNamePart );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<StackTraceElement> classIs(final Class cls)
		 public static System.Predicate<StackTraceElement> ClassIs( Type cls )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return item => item.ClassName.Equals( cls.FullName );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<StackTraceElement> classNameAndMethodAre(final String className, final String methodName)
		 public static System.Predicate<StackTraceElement> ClassNameAndMethodAre( string className, string methodName )
		 {
			  return item => item.ClassName.Equals( className ) && item.MethodName.Equals( methodName );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<StackTraceElement> classAndMethodAre(final Class cls, final String methodName)
		 public static System.Predicate<StackTraceElement> ClassAndMethodAre( Type cls, string methodName )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return item => item.ClassName.Equals( cls.FullName ) && item.MethodName.Equals( methodName );
		 }

		 public static System.Predicate<StackTraceElement> MethodIs( string methodName )
		 {
			  return item => item.MethodName.Equals( methodName );
		 }

		 public class StackTracer
		 {
			  internal readonly IDictionary<CallStack, AtomicInteger> UniqueStackTraces = new Dictionary<CallStack, AtomicInteger>();
			  internal bool ConsiderMessages = true;

			  /// <summary>
			  /// Returns <seealso cref="AtomicInteger"/> for the unique stack trace provided. It gets updated
			  /// as more are added.
			  /// </summary>
			  public virtual AtomicInteger Add( Exception t )
			  {
					CallStack key = new CallStack( t, ConsiderMessages );
					AtomicInteger count = UniqueStackTraces.computeIfAbsent( key, k => new AtomicInteger() );
					count.incrementAndGet();
					return count;
			  }

			  public virtual void Print( PrintStream @out, int interestThreshold )
			  {
					Console.WriteLine( "Printing stack trace counts:" );
					long total = 0;
					foreach ( KeyValuePair<CallStack, AtomicInteger> entry in UniqueStackTraces.SetOfKeyValuePairs() )
					{
						 if ( entry.Value.get() >= interestThreshold )
						 {
							  @out.println( entry.Value + " times:" );
							  entry.Key.stackTrace.printStackTrace( @out );
						 }
						 total += entry.Value.get();
					}
					@out.println( "------" );
					@out.println( "Total:" + total );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public StackTracer printAtShutdown(final java.io.PrintStream out, final int interestThreshold)
			  public virtual StackTracer PrintAtShutdown( PrintStream @out, int interestThreshold )
			  {
					Runtime.Runtime.addShutdownHook( new Thread( () => print(@out, interestThreshold) ) );
					return this;
			  }

			  public virtual StackTracer IgnoreMessages()
			  {
					ConsiderMessages = false;
					return this;
			  }
		 }

		 public class CallStack
		 {
			  internal readonly string Message;
			  internal readonly Exception StackTrace;
			  internal readonly StackTraceElement[] Elements;
			  internal readonly bool ConsiderMessage;

			  public CallStack( Exception stackTrace, bool considerMessage )
			  {
					this.Message = stackTrace.Message;
					this.StackTrace = stackTrace;
					this.ConsiderMessage = considerMessage;
					this.Elements = stackTrace.StackTrace;
			  }

			  public CallStack( StackTraceElement[] elements, string message )
			  {
					this.Message = message;
					this.StackTrace = null;
					this.Elements = elements;
					this.ConsiderMessage = true;
			  }

			  public override int GetHashCode()
			  {
					int hashCode = string.ReferenceEquals( Message, null ) || !ConsiderMessage ? 31 : Message.GetHashCode();
					foreach ( StackTraceElement element in Elements )
					{
						 hashCode = hashCode * 9 + element.GetHashCode();
					}
					return hashCode;
			  }

			  public override bool Equals( object obj )
			  {
					if ( !( obj is CallStack ) )
					{
						 return false;
					}

					CallStack o = ( CallStack ) obj;
					if ( ConsiderMessage )
					{
						 if ( string.ReferenceEquals( Message, null ) )
						 {
							  if ( !string.ReferenceEquals( o.Message, null ) )
							  {
									return false;
							  }
						 }
						 else if ( !Message.Equals( o.Message ) )
						 {
							  return false;
						 }
					}
					if ( Elements.Length != o.Elements.Length )
					{
						 return false;
					}
					for ( int i = 0; i < Elements.Length; i++ )
					{
						 if ( !Elements[i].Equals( o.Elements[i] ) )
						 {
							  return false;
						 }
					}
					return true;
			  }

			  public override string ToString()
			  {
					StringBuilder builder = new StringBuilder();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					builder.Append( StackTrace != null ? StackTrace.GetType().FullName + ": " : "" ).Append(!string.ReferenceEquals(Message, null) ? Message : "");
					foreach ( StackTraceElement element in Elements )
					{
						 builder.Append( format( "%n" ) ).Append( "    at " ).Append( element.ToString() );
					}
					return builder.ToString();
			  }
		 }

		 private static bool EnabledAssertions()
		 {
			  bool enabled = false;
			  //noinspection AssertWithSideEffects,ConstantConditions
			  Debug.Assert( enabled = true, "A trick to set this variable to true if assertions are enabled" );
			  //noinspection ConstantConditions
			  return enabled;
		 }

		 /// <summary>
		 /// Super simple utility for determining where most time is spent when you don't know where to even start.
		 /// It could be used to home in on right place in a test or in a sequence of operations or similar.
		 /// </summary>
		 public abstract class Timer
		 {
			  internal readonly TimeUnit Unit;
			  internal long StartTime;

			  protected internal Timer( TimeUnit unit )
			  {
					this.Unit = unit;
					this.StartTime = CurrentTime();
			  }

			  protected internal abstract long CurrentTime();

			  public virtual void Reset()
			  {
					StartTime = CurrentTime();
			  }

			  public virtual void At( string point )
			  {
					long duration = CurrentTime() - StartTime;
					Console.WriteLine( duration( Unit.toMillis( duration ) ) + " @ " + point );
					StartTime = CurrentTime();
			  }

			  public static Timer Millis()
			  {
					return new Millis();
			  }

			  private class Millis : Timer
			  {
					internal Millis() : base(TimeUnit.MILLISECONDS)
					{
					}

					protected internal override long CurrentTime()
					{
						 return currentTimeMillis();
					}
			  }

			  public static Timer Nanos()
			  {
					return new Nanos();
			  }

			  private class Nanos : Timer
			  {
					internal Nanos() : base(TimeUnit.NANOSECONDS)
					{
					}

					protected internal override long CurrentTime()
					{
						 return nanoTime();
					}
			  }
		 }

		 public static long Time( long startTime, string message )
		 {
			  Console.WriteLine( duration( currentTimeMillis() - startTime ) + ": " + message );
			  return currentTimeMillis();
		 }
	}

}