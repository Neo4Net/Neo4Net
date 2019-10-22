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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using Neo4Net.Kernel.ha;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class UpdatePullerSwitcherTest
	{
		 private UpdatePullerSwitcher _modeSwitcher;
		 private SlaveUpdatePuller _slaveUpdatePuller;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.kernel.ha.DelegateInvocationHandler<org.Neo4Net.kernel.ha.UpdatePuller> invocationHandler = mock(org.Neo4Net.kernel.ha.DelegateInvocationHandler.class);
			  DelegateInvocationHandler<UpdatePuller> invocationHandler = mock( typeof( DelegateInvocationHandler ) );
			  PullerFactory pullerFactory = mock( typeof( PullerFactory ) );
			  _slaveUpdatePuller = mock( typeof( SlaveUpdatePuller ) );
			  when( pullerFactory.CreateSlaveUpdatePuller() ).thenReturn(_slaveUpdatePuller);
			  when( invocationHandler.setDelegate( _slaveUpdatePuller ) ).thenReturn( _slaveUpdatePuller );

			  _modeSwitcher = new UpdatePullerSwitcher( invocationHandler, pullerFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterUpdatePuller()
		 public virtual void MasterUpdatePuller()
		 {
			  UpdatePuller masterPuller = _modeSwitcher.MasterImpl;
			  assertSame( MasterUpdatePuller.INSTANCE, masterPuller );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveUpdatePuller()
		 public virtual void SlaveUpdatePuller()
		 {
			  UpdatePuller updatePuller = _modeSwitcher.SlaveImpl;
			  assertSame( _slaveUpdatePuller, updatePuller );
			  verifyZeroInteractions( _slaveUpdatePuller );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchToPendingTest()
		 public virtual void SwitchToPendingTest()
		 {
			  _modeSwitcher.switchToSlave();
			  verify( _slaveUpdatePuller ).start();

			  _modeSwitcher.switchToSlave();
			  InOrder inOrder = inOrder( _slaveUpdatePuller );
			  inOrder.verify( _slaveUpdatePuller ).stop();
			  inOrder.verify( _slaveUpdatePuller ).start();
		 }
	}

}