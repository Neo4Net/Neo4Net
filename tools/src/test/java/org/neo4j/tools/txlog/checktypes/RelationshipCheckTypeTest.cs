﻿/*
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
namespace Org.Neo4j.tools.txlog.checktypes
{
	using Test = org.junit.Test;

	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class RelationshipCheckTypeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void inUseRecordEquality()
		 public virtual void InUseRecordEquality()
		 {
			  RelationshipRecord record1 = new RelationshipRecord( 1 );
			  record1.Initialize( true, 1, 2, 3, 4, 5, 6, 7, 8, true, false );
			  record1.SecondaryUnitId = 42;

			  RelationshipRecord record2 = record1.Clone();

			  RelationshipCheckType check = new RelationshipCheckType();

			  assertTrue( check.Equal( record1, record2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notInUseRecordEquality()
		 public virtual void NotInUseRecordEquality()
		 {
			  RelationshipRecord record1 = new RelationshipRecord( 1 );
			  record1.Initialize( false, 1, 2, 3, 4, 5, 6, 7, 8, true, false );
			  record1.SecondaryUnitId = 42;

			  RelationshipRecord record2 = new RelationshipRecord( 1 );
			  record2.Initialize( false, 11, 22, 33, 44, 55, 66, 77, 88, false, true );
			  record2.SecondaryUnitId = 24;

			  RelationshipCheckType check = new RelationshipCheckType();

			  assertTrue( check.Equal( record1, record2 ) );
		 }
	}

}