using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.helper
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Neo4Net.Functions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class SuspendableLifeCycleFailingTest
	{

		 private CountingThrowingSuspendableLifeCycle _lifeCycle;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _lifeCycle = new CountingThrowingSuspendableLifeCycle();
			  _lifeCycle.init();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEnableIfStopFailed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanEnableIfStopFailed()
		 {
			  _lifeCycle.start();
			  _lifeCycle.setFailMode();

			  RunFailing( SuspendableLifeCycle.stop );

			  _lifeCycle.setSuccessMode();

			  _lifeCycle.enable();

			  assertEquals( 2, _lifeCycle.starts );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canEnableIfShutdownFailed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanEnableIfShutdownFailed()
		 {
			  _lifeCycle.start();
			  _lifeCycle.setFailMode();

			  RunFailing( SuspendableLifeCycle.shutdown );

			  _lifeCycle.setSuccessMode();

			  _lifeCycle.enable();

			  assertEquals( 2, _lifeCycle.starts );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canStartifDisableFailed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanStartifDisableFailed()
		 {
			  _lifeCycle.setFailMode();
			  RunFailing( SuspendableLifeCycle.disable );

			  _lifeCycle.setSuccessMode();
			  _lifeCycle.start();

			  assertEquals( 1, _lifeCycle.starts );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void runFailing(Neo4Net.function.ThrowingConsumer<SuspendableLifeCycle,Throwable> consumer) throws Throwable
		 private void RunFailing( ThrowingConsumer<SuspendableLifeCycle, Exception> consumer )
		 {
			  try
			  {
					consumer.Accept( _lifeCycle );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
			  }
		 }
	}

}