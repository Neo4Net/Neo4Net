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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input
{
	using Test = org.junit.Test;

	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using Monitor = Org.Neo4j.@unsafe.Impl.Batchimport.ImportLogic.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class EstimationSanityCheckerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutCountGettingCloseToCapacity()
		 public virtual void ShouldWarnAboutCountGettingCloseToCapacity()
		 {
			  // given
			  RecordFormats formats = Standard.LATEST_RECORD_FORMATS;
			  Monitor monitor = mock( typeof( Monitor ) );
			  Input_Estimates estimates = Inputs.KnownEstimates( formats.Node().MaxId - 1000, formats.Relationship().MaxId - 1000, 0, 0, 0, 0, 0 ); // we don't care about the rest of the estimates in this checking

			  // when
			  ( new EstimationSanityChecker( formats, monitor ) ).SanityCheck( estimates );

			  // then
			  verify( monitor ).mayExceedNodeIdCapacity( formats.Node().MaxId, estimates.NumberOfNodes() );
			  verify( monitor ).mayExceedRelationshipIdCapacity( formats.Relationship().MaxId, estimates.NumberOfRelationships() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutCounthigherThanCapacity()
		 public virtual void ShouldWarnAboutCounthigherThanCapacity()
		 {
			  // given
			  RecordFormats formats = Standard.LATEST_RECORD_FORMATS;
			  Monitor monitor = mock( typeof( Monitor ) );
			  Input_Estimates estimates = Inputs.KnownEstimates( formats.Node().MaxId * 2, formats.Relationship().MaxId * 2, 0, 0, 0, 0, 0 ); // we don't care about the rest of the estimates in this checking

			  // when
			  ( new EstimationSanityChecker( formats, monitor ) ).SanityCheck( estimates );

			  // then
			  verify( monitor ).mayExceedNodeIdCapacity( formats.Node().MaxId, estimates.NumberOfNodes() );
			  verify( monitor ).mayExceedRelationshipIdCapacity( formats.Relationship().MaxId, estimates.NumberOfRelationships() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWantIfCountWayLowerThanCapacity()
		 public virtual void ShouldNotWantIfCountWayLowerThanCapacity()
		 {
			  // given
			  RecordFormats formats = Standard.LATEST_RECORD_FORMATS;
			  Monitor monitor = mock( typeof( Monitor ) );
			  Input_Estimates estimates = Inputs.KnownEstimates( 1000, 1000, 0, 0, 0, 0, 0 ); // we don't care about the rest of the estimates in this checking

			  // when
			  ( new EstimationSanityChecker( formats, monitor ) ).SanityCheck( estimates );

			  // then
			  verifyNoMoreInteractions( monitor );
		 }
	}

}