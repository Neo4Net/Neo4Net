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
namespace Neo4Net.tools.txlog.checktypes
{
	using Test = org.junit.Test;

	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;

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