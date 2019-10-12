using System;

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
namespace Org.Neo4j.Io.pagecache.randomharness
{

	using Profiler = Org.Neo4j.Resources.Profiler;

	internal class PlanRunner : Callable<Void>
	{
		 private readonly Plan _plan;
		 private readonly AtomicBoolean _stopSignal;
		 private readonly Profiler _profiler;

		 internal PlanRunner( Plan plan, AtomicBoolean stopSignal, Profiler profiler )
		 {
			  this._plan = plan;
			  this._stopSignal = stopSignal;
			  this._profiler = profiler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void call() throws Exception
		 public override Void Call()
		 {
			  using ( Org.Neo4j.Resources.Profiler_ProfiledInterval profilingRun = _profiler.profile() )
			  {
					Action action = _plan.next();
					while ( action != null && !_stopSignal.get() )
					{
						 try
						 {
							  action.Perform();
						 }
						 catch ( Exception )
						 {
						 }
						 action = _plan.next();
					}
					return null;
			  }
		 }
	}

}