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
namespace Org.Neo4j.Kernel.impl.transaction.log.checkpoint
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class AbstractCheckPointThresholdTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallConsumerProvidingTheDescriptionWhenThresholdIsTrue()
		 public virtual void ShouldCallConsumerProvidingTheDescriptionWhenThresholdIsTrue()
		 {
			  // Given
			  string description = "description";
			  AbstractCheckPointThreshold threshold = new TheAbstractCheckPointThreshold( true, description );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> calledWith = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<string> calledWith = new AtomicReference<string>();
			  // When
			  threshold.IsCheckPointingNeeded( 42, calledWith.set );

			  // Then
			  assertEquals( description, calledWith.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCallConsumerProvidingTheDescriptionWhenThresholdIsFalse()
		 public virtual void ShouldNotCallConsumerProvidingTheDescriptionWhenThresholdIsFalse()
		 {
			  // Given
			  AbstractCheckPointThreshold threshold = new TheAbstractCheckPointThreshold( false, null );

			  // When
			  threshold.IsCheckPointingNeeded(42, s =>
			  {
				throw new System.InvalidOperationException( "nooooooooo!" );
			  });

			  // Then
			  // should not throw
		 }

		 private class TheAbstractCheckPointThreshold : AbstractCheckPointThreshold
		 {
			  internal readonly bool Reached;

			  internal TheAbstractCheckPointThreshold( bool reached, string description ) : base( description )
			  {
					this.Reached = reached;
			  }

			  public override void Initialize( long transactionId )
			  {

			  }

			  public override void CheckPointHappened( long transactionId )
			  {

			  }

			  public override long CheckFrequencyMillis()
			  {
					return CheckPointThreshold_Fields.DefaultCheckingFrequencyMillis;
			  }

			  protected internal override bool ThresholdReached( long lastCommittedTransactionId )
			  {
					return Reached;
			  }
		 }
	}

}