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
namespace Org.Neo4j.Test.rule.dump
{
	using Matcher = org.hamcrest.Matcher;
	using ExternalResource = org.junit.rules.ExternalResource;


	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

	public class DumpProcessInformationRule : ExternalResource
	{
		 public interface Dump
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dump() throws Exception;
			  void Dump();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Dump localVm(final java.io.PrintStream out)
		 public static Dump LocalVm( PrintStream @out )
		 {
			  return () => DumpVmInformation.dumpVmInfo(@out);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Dump otherVm(final org.hamcrest.Matcher<String> processFilter, final java.io.File baseDir)
		 public static Dump OtherVm( Matcher<string> processFilter, File baseDir )
		 {
			  return () => (new DumpProcessInformation(NullLogProvider.Instance, baseDir)).doThreadDump(processFilter);
		 }

		 private readonly ScheduledExecutorService _executor = Executors.newScheduledThreadPool( 2 );
		 private readonly long _duration;
		 private readonly TimeUnit _timeUnit;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private volatile java.util.concurrent.ScheduledFuture<?> thunk;
		 private volatile ScheduledFuture<object> _thunk;
		 private readonly Dump[] _dumps;

		 /// <summary>
		 /// Dumps process information about processes on the local machine, filtered by processFilter
		 /// </summary>
		 public DumpProcessInformationRule( long duration, TimeUnit timeUnit, params Dump[] dumps )
		 {
			  this._duration = duration;
			  this._timeUnit = timeUnit;
			  this._dumps = dumps;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected synchronized void before() throws Throwable
		 protected internal override void Before()
		 {
			 lock ( this )
			 {
				  if ( null == _thunk )
				  {
						base.Before();
						_thunk = _executor.schedule((Callable<Void>)() =>
						{
						 Dump();
						 return null;
						}, _duration, _timeUnit);
				  }
				  else
				  {
						throw new System.InvalidOperationException( "process dumping thunk already started" );
				  }
			 }
		 }

		 protected internal override void After()
		 {
			 lock ( this )
			 {
				  if ( null != _thunk && !_thunk.Done )
				  {
						_thunk.cancel( true );
				  }
				  _thunk = null;
				  base.After();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dump() throws Exception
		 public virtual void Dump()
		 {
			  foreach ( Dump dump in _dumps )
			  {
					dump.DumpConflict();
			  }
		 }
	}

}