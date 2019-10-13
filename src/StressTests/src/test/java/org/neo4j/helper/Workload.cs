﻿using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.helper
{
	using Control = Neo4Net.causalclustering.stresstests.Control;

	public abstract class Workload : ThreadStart
	{
		 protected internal readonly Control Control;
		 private readonly long _sleepTimeMillis;

		 public Workload( Control control ) : this( control, 0 )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public Workload(org.neo4j.causalclustering.stresstests.Control control, long sleepTimeMillis)
		 public Workload( Control control, long sleepTimeMillis )
		 {
			  this.Control = control;
			  this._sleepTimeMillis = sleepTimeMillis;
		 }

		 public override void Run()
		 {
			  try
			  {
					while ( Control.keepGoing() )
					{
						 DoWork();
						 if ( _sleepTimeMillis != 0 )
						 {
							  Thread.Sleep( _sleepTimeMillis );
						 }
					}
			  }
			  catch ( InterruptedException )
			  {
					Thread.interrupted();
			  }
			  catch ( Exception t )
			  {
					Control.onFailure( t );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void doWork() throws Exception;
		 protected internal abstract void DoWork();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("RedundantThrows") public void prepare() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Prepare()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("RedundantThrows") public void validate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Validate()
		 {
		 }
	}

}