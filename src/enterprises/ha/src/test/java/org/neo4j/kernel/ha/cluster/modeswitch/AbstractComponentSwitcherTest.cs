using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using Test = org.junit.Test;

	using Neo4Net.Kernel.ha;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class AbstractComponentSwitcherTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchesToMaster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchesToMaster()
		 {
			  DelegateInvocationHandler<Component> @delegate = new DelegateInvocationHandler<Component>( typeof( Component ) );
			  TestComponentSwitcher switcher = new TestComponentSwitcher( @delegate );

			  switcher.SwitchToMaster();

			  assertEquals( DelegateClass( @delegate ), typeof( MasterComponent ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchesToSlave() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchesToSlave()
		 {
			  DelegateInvocationHandler<Component> @delegate = new DelegateInvocationHandler<Component>( typeof( Component ) );
			  TestComponentSwitcher switcher = new TestComponentSwitcher( @delegate );

			  switcher.SwitchToSlave();

			  assertEquals( DelegateClass( @delegate ), typeof( SlaveComponent ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void switchesToPending() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SwitchesToPending()
		 {
			  DelegateInvocationHandler<Component> @delegate = new DelegateInvocationHandler<Component>( typeof( Component ) );
			  TestComponentSwitcher switcher = new TestComponentSwitcher( @delegate );

			  switcher.SwitchToPending();

			  assertEquals( DelegateClass( @delegate ), typeof( PendingComponent ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Class delegateClass(org.neo4j.kernel.ha.DelegateInvocationHandler<?> invocationHandler) throws Throwable
		 private static Type DelegateClass<T1>( DelegateInvocationHandler<T1> invocationHandler )
		 {
			  return ( Type ) invocationHandler.Invoke( new object(), typeof(object).GetMethod("getClass"), new object[0] );
		 }

		 private class TestComponentSwitcher : AbstractComponentSwitcher<Component>
		 {
			  internal TestComponentSwitcher( DelegateInvocationHandler<Component> @delegate ) : base( @delegate )
			  {
			  }

			  protected internal override Component MasterImpl
			  {
				  get
				  {
						return new MasterComponent();
				  }
			  }

			  protected internal override Component SlaveImpl
			  {
				  get
				  {
						return new SlaveComponent();
				  }
			  }

			  protected internal override Component PendingImpl
			  {
				  get
				  {
						return new PendingComponent();
				  }
			  }
		 }

		 private interface Component
		 {
		 }

		 private class MasterComponent : Component
		 {
		 }

		 private class SlaveComponent : Component
		 {
		 }

		 private class PendingComponent : Component
		 {
		 }
	}

}