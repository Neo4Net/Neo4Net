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

	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class NeoStoreCheckTypeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void inUseRecordEquality()
		 public virtual void InUseRecordEquality()
		 {
			  NeoStoreRecord record1 = new NeoStoreRecord();
			  record1.Initialize( true, 1 );

			  NeoStoreRecord record2 = record1.Clone();

			  NeoStoreCheckType check = new NeoStoreCheckType();

			  assertTrue( check.Equal( record1, record2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notInUseRecordEquality()
		 public virtual void NotInUseRecordEquality()
		 {
			  NeoStoreRecord record1 = new NeoStoreRecord();
			  record1.Initialize( false, 1 );

			  NeoStoreRecord record2 = new NeoStoreRecord();
			  record2.Initialize( false, 11 );

			  NeoStoreCheckType check = new NeoStoreCheckType();

			  assertTrue( check.Equal( record1, record2 ) );
		 }
	}

}