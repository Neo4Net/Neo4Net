using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.store.id
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using CommunityIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IdTypeConfiguration = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;

	public class DefaultIdGeneratorFactory : IdGeneratorFactory
	{
		 private readonly Dictionary<IdType, IdGenerator> _generators = new Dictionary<IdType, IdGenerator>( typeof( IdType ) );
		 private readonly FileSystemAbstraction _fs;
		 private readonly IdTypeConfigurationProvider _idTypeConfigurationProvider;

		 public DefaultIdGeneratorFactory( FileSystemAbstraction fs ) : this( fs, new CommunityIdTypeConfigurationProvider() )
		 {
		 }

		 public DefaultIdGeneratorFactory( FileSystemAbstraction fs, IdTypeConfigurationProvider idTypeConfigurationProvider )
		 {
			  this._fs = fs;
			  this._idTypeConfigurationProvider = idTypeConfigurationProvider;
		 }

		 public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdTypeConfiguration idTypeConfiguration = _idTypeConfigurationProvider.getIdTypeConfiguration( idType );
			  return Open( filename, idTypeConfiguration.GrabSize, idType, highId, maxId );
		 }

		 public override IdGenerator Open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdTypeConfiguration idTypeConfiguration = _idTypeConfigurationProvider.getIdTypeConfiguration( idType );
			  IdGenerator generator = Instantiate( _fs, fileName, grabSize, maxId, idTypeConfiguration.AllowAggressiveReuse(), idType, highId );
			  _generators[idType] = generator;
			  return generator;
		 }

		 protected internal virtual IdGenerator Instantiate( FileSystemAbstraction fs, File fileName, int grabSize, long maxValue, bool aggressiveReuse, IdType idType, System.Func<long> highId )
		 {
			  return new IdGeneratorImpl( fs, fileName, grabSize, maxValue, aggressiveReuse, idType, highId );
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  return _generators[idType];
		 }

		 public override void Create( File fileName, long highId, bool throwIfFileExists )
		 {
			  IdGeneratorImpl.CreateGenerator( _fs, fileName, highId, throwIfFileExists );
		 }
	}

}