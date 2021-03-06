﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.state.machines.id
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using IdTypeConfiguration = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class ReplicatedIdGeneratorFactory : IdGeneratorFactory
	{
		 private readonly IDictionary<IdType, ReplicatedIdGenerator> _generators = new Dictionary<IdType, ReplicatedIdGenerator>();
		 private readonly FileSystemAbstraction _fs;
		 private readonly ReplicatedIdRangeAcquirer _idRangeAcquirer;
		 private readonly LogProvider _logProvider;
		 private IdTypeConfigurationProvider _idTypeConfigurationProvider;

		 public ReplicatedIdGeneratorFactory( FileSystemAbstraction fs, ReplicatedIdRangeAcquirer idRangeAcquirer, LogProvider logProvider, IdTypeConfigurationProvider idTypeConfigurationProvider )
		 {
			  this._fs = fs;
			  this._idRangeAcquirer = idRangeAcquirer;
			  this._logProvider = logProvider;
			  this._idTypeConfigurationProvider = idTypeConfigurationProvider;
		 }

		 public override IdGenerator Open( File file, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdTypeConfiguration idTypeConfiguration = _idTypeConfigurationProvider.getIdTypeConfiguration( idType );
			  return OpenGenerator( file, idTypeConfiguration.GrabSize, idType, highId, maxId, idTypeConfiguration.AllowAggressiveReuse() );
		 }

		 public override IdGenerator Open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdTypeConfiguration idTypeConfiguration = _idTypeConfigurationProvider.getIdTypeConfiguration( idType );
			  return OpenGenerator( fileName, grabSize, idType, highId, maxId, idTypeConfiguration.AllowAggressiveReuse() );
		 }

		 private IdGenerator OpenGenerator( File file, int grabSize, IdType idType, System.Func<long> highId, long maxId, bool aggressiveReuse )
		 {
			  ReplicatedIdGenerator other = _generators[idType];
			  if ( other != null )
			  {
					other.Dispose();
			  }
			  ReplicatedIdGenerator replicatedIdGenerator = new ReplicatedIdGenerator( _fs, file, idType, highId, _idRangeAcquirer, _logProvider, grabSize, aggressiveReuse );

			  _generators[idType] = replicatedIdGenerator;
			  return replicatedIdGenerator;
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  return _generators[idType];
		 }

		 public override void Create( File fileName, long highId, bool throwIfFileExists )
		 {
			  ReplicatedIdGenerator.CreateGenerator( _fs, fileName, highId, throwIfFileExists );
		 }
	}

}