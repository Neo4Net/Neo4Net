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

	internal class StrategyResolverService
	{
		 private readonly BackupStrategyWrapper _haBackupStrategy;
		 private readonly BackupStrategyWrapper _ccBackupStrategy;

		 internal StrategyResolverService( BackupStrategyWrapper haBackupStrategy, BackupStrategyWrapper ccBackupStrategy )
		 {
			  this._haBackupStrategy = haBackupStrategy;
			  this._ccBackupStrategy = ccBackupStrategy;
		 }

		 internal virtual IList<BackupStrategyWrapper> GetStrategies( SelectedBackupProtocol selectedBackupProtocol )
		 {
			  switch ( selectedBackupProtocol.innerEnumValue )
			  {
			  case Org.Neo4j.backup.impl.SelectedBackupProtocol.InnerEnum.ANY:
					return Arrays.asList( _ccBackupStrategy, _haBackupStrategy );
			  case Org.Neo4j.backup.impl.SelectedBackupProtocol.InnerEnum.COMMON:
					return Collections.singletonList( _haBackupStrategy );
			  case Org.Neo4j.backup.impl.SelectedBackupProtocol.InnerEnum.CATCHUP:
					return Collections.singletonList( _ccBackupStrategy );
			  default:
					throw new System.ArgumentException( "Unhandled protocol choice: " + selectedBackupProtocol );
			  }
		 }
	}

}