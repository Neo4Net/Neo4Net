using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Lifecycle
{

	public class Lifecycles
	{
		 private Lifecycles()
		 { // No instances allowed or even necessary
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Lifecycle multiple(final Iterable<? extends Lifecycle> lifecycles)
		 public static Lifecycle Multiple<T1>( IEnumerable<T1> lifecycles ) where T1 : Lifecycle
		 {
			  return new CombinedLifecycle( lifecycles );
		 }

		 public static Lifecycle Multiple( params Lifecycle[] lifecycles )
		 {
			  return Multiple( Arrays.asList( lifecycles ) );
		 }

		 private class CombinedLifecycle : Lifecycle
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Iterable<? extends Lifecycle> lifecycles;
			  internal readonly IEnumerable<Lifecycle> Lifecycles;

			  internal CombinedLifecycle<T1>( IEnumerable<T1> lifecycles ) where T1 : Lifecycle
			  {
					this.Lifecycles = lifecycles;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
			  public override void Init()
			  {
					foreach ( Lifecycle lifecycle in Lifecycles )
					{
						 lifecycle.Init();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
			  public override void Start()
			  {
					foreach ( Lifecycle lifecycle in Lifecycles )
					{
						 lifecycle.Start();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
			  public override void Stop()
			  {
					foreach ( Lifecycle lifecycle in Lifecycles )
					{
						 lifecycle.Stop();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
			  public override void Shutdown()
			  {
					foreach ( Lifecycle lifecycle in Lifecycles )
					{
						 lifecycle.Shutdown();
					}
			  }
		 }
	}

}