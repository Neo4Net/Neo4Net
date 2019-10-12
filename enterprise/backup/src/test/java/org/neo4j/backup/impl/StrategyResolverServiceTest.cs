using System.Collections.Generic;

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
namespace Org.Neo4j.backup.impl
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class StrategyResolverServiceTest
	{

		 internal StrategyResolverService Subject;
		 internal BackupStrategyWrapper HaBackupStrategy;
		 internal BackupStrategyWrapper CcBackupStrategy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  HaBackupStrategy = mock( typeof( BackupStrategyWrapper ) );
			  CcBackupStrategy = mock( typeof( BackupStrategyWrapper ) );
			  Subject = new StrategyResolverService( HaBackupStrategy, CcBackupStrategy );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void anyProvidesBothStrategiesCorrectOrder()
		 public virtual void AnyProvidesBothStrategiesCorrectOrder()
		 {
			  IList<BackupStrategyWrapper> result = Subject.getStrategies( SelectedBackupProtocol.Any );
			  assertEquals( Arrays.asList( CcBackupStrategy, HaBackupStrategy ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void legacyProvidesBackupProtocol()
		 public virtual void LegacyProvidesBackupProtocol()
		 {
			  IList<BackupStrategyWrapper> result = Subject.getStrategies( SelectedBackupProtocol.Common );
			  assertEquals( Collections.singletonList( HaBackupStrategy ), result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void catchupProvidesTransactionProtocol()
		 public virtual void CatchupProvidesTransactionProtocol()
		 {
			  IList<BackupStrategyWrapper> result = Subject.getStrategies( SelectedBackupProtocol.Catchup );
			  assertEquals( Collections.singletonList( CcBackupStrategy ), result );
		 }
	}

}