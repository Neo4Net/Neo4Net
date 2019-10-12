using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Resources
{

	/// <summary>
	/// An API for profiling threads, to see where they spend their time.
	/// <para>
	/// The profiler is created with the <seealso cref="profiler()"/> method, and is ready to use. Threads are profiled individually, and profiling is started with the
	/// <seealso cref="profile()"/> family of methods. Profiling can be selectively stopped by closing the returned <seealso cref="ProfiledInterval"/> instance. When you are done
	/// collecting profiling data, call <seealso cref="finish()"/>. The <seealso cref="finish()"/> mehtod must be called before the profiling data can be printed, and calling
	/// <seealso cref="finish()"/> will cause all on-going profiling to stop. Once the profiling has finished, the profile data can be printed with the
	/// <seealso cref="printProfile(PrintStream, string)"/> method.
	/// </para>
	/// <para>
	/// If you want to use the profiler again, you must call <seealso cref="reset()"/> before you can start profiling again.
	/// </para>
	/// </summary>
	public interface Profiler
	{
		 /// <returns> a <seealso cref="Profiler"/> that does not actually do any profiling. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static Profiler nullProfiler()
	//	 {
	//		  return new NullProfiler();
	//	 }

		 /// <returns> a stack-sampling <seealso cref="Profiler"/>. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static Profiler profiler()
	//	 {
	//		  return new SamplingProfiler();
	//	 }

		 /// <summary>
		 /// Reset the state of the profiler, and clear out all collected profile data.
		 /// <para>
		 /// Call this if you want to use the profiler again, after having collected profiling data, <seealso cref="finish() finished"/> profiling,
		 /// and <seealso cref="printProfile(PrintStream, string) printed"/> your profiling data.
		 /// </para>
		 /// </summary>
		 void Reset();

		 /// <summary>
		 /// Set the sampling interval, as the desired nanoseconds between samples. </summary>
		 /// <param name="nanos"> The desired nanoseconds between profiling samples. </param>
		 long SampleIntervalNanos { set; }

		 /// <summary>
		 /// Start profiling the current thread.
		 /// <para>
		 /// The profiled thread will have its stack sampled, until either the test ends, or the returned resource is closed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> A resource that, when closed, will stop the profiling of the current thread. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Profiler_ProfiledInterval profile()
	//	 {
	//		  return profile(Thread.currentThread());
	//	 }

		 /// <summary>
		 /// Start profiling the given thread.
		 /// <para>
		 /// The profiled thread will have its stack sampled, until either the test ends, or the returned resource is closed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="threadToProfile"> The thread to profile. </param>
		 /// <returns> A resource that, when closed, will stop profiling the given thread. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default Profiler_ProfiledInterval profile(Thread threadToProfile)
	//	 {
	//		  return profile(threadToProfile, 0);
	//	 }

		 /// <summary>
		 /// Finish the profiling, which includes stopping and waiting for the termination of all on-going profiles, and then prepare the collected profiling data
		 /// for printing. This method must be called before <seealso cref="printProfile(PrintStream, string)"/> can be called.
		 /// </summary>
		 /// <exception cref="InterruptedException"> If the thread was interrupted while waiting for all on-going profiling activities to stop. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void finish() throws InterruptedException;
		 void Finish();

		 /// <summary>
		 /// Write out a textual representation of the collected profiling data, to the given <seealso cref="PrintStream"/>.
		 /// <para>
		 /// The report will start with the given profile title, and will have a "sorted sample tree" printed for each of the profiled threads.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="out"> the print stream where the output will be written to. </param>
		 /// <param name="profileTitle"> the title of the profile report. </param>
		 void PrintProfile( PrintStream @out, string profileTitle );

		 /// <summary>
		 /// Start profiling the given thread after the given delay in nanoseconds.
		 /// <para>
		 /// The profiled thread will have its stack sampled, until either the test ends, or the returned resource is closed.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="threadToProfile"> The thread to profile. </param>
		 /// <param name="initialDelayNanos"> The profiling will not start until after this delay in nanoseconds has transpired. </param>
		 /// <returns> A resource that, when closed, will stop the profiling of the given thread. </returns>
		 Profiler_ProfiledInterval Profile( Thread threadToProfile, long initialDelayNanos );

		 /// <summary>
		 /// When closed, will cause the on-going profiling of a thread to be stopped.
		 /// </summary>
	}

	 public interface Profiler_ProfiledInterval : AutoCloseable
	 {
		  void Close();
	 }

}