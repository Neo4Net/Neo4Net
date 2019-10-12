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

	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// <seealso cref="IdGeneratorFactory"/> managing read-only <seealso cref="IdGenerator"/> instances which basically only can access
	/// <seealso cref="IdGenerator.getHighId()"/> and <seealso cref="IdGenerator.getHighestPossibleIdInUse()"/>.
	/// </summary>
	public class ReadOnlyIdGeneratorFactory : IdGeneratorFactory
	{
		 private readonly Dictionary<IdType, IdGenerator> _idGenerators = new Dictionary<IdType, IdGenerator>( typeof( IdType ) );
		 private readonly FileSystemAbstraction _fileSystemAbstraction;

		 public ReadOnlyIdGeneratorFactory()
		 {
			  this._fileSystemAbstraction = new DefaultFileSystemAbstraction();
		 }

		 public ReadOnlyIdGeneratorFactory( FileSystemAbstraction fileSystemAbstraction )
		 {
			  this._fileSystemAbstraction = fileSystemAbstraction;
		 }

		 public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
		 {
			  return Open( filename, 0, idType, highId, maxId );
		 }

		 public override IdGenerator Open( File filename, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdGenerator generator = new ReadOnlyIdGenerator( highId, _fileSystemAbstraction, filename );
			  _idGenerators[idType] = generator;
			  return generator;
		 }

		 public override void Create( File filename, long highId, bool throwIfFileExists )
		 {
			  // Don't
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  return _idGenerators[idType];
		 }

		 internal class ReadOnlyIdGenerator : IdGenerator
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long HighIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long DefragCountConflict;

			  internal ReadOnlyIdGenerator( System.Func<long> highId, FileSystemAbstraction fs, File filename )
			  {
					if ( fs != null && fs.FileExists( filename ) )
					{
						 try
						 {
							  this.HighIdConflict = IdGeneratorImpl.ReadHighId( fs, filename );
							  DefragCountConflict = IdGeneratorImpl.ReadDefragCount( fs, filename );
						 }
						 catch ( IOException e )
						 {
							  throw new UnderlyingStorageException( "Failed to read id counts of the id file: " + filename, e );
						 }
					}
					else
					{
						 this.HighIdConflict = highId();
						 DefragCountConflict = 0;
					}
			  }

			  public override long NextId()
			  {
					throw new System.NotSupportedException();
			  }

			  public override IdRange NextIdBatch( int size )
			  {
					throw new System.NotSupportedException();
			  }

			  public virtual long HighId
			  {
				  set
				  { // OK
				  }
				  get
				  {
						return HighIdConflict;
				  }
			  }


			  public virtual long HighestPossibleIdInUse
			  {
				  get
				  {
						return HighIdConflict - 1;
				  }
			  }

			  public override void FreeId( long id )
			  { // Don't
			  }

			  public override void Close()
			  { // Nothing to close
			  }

			  public virtual long NumberOfIdsInUse
			  {
				  get
				  {
						return HighIdConflict - DefragCountConflict;
				  }
			  }

			  public virtual long DefragCount
			  {
				  get
				  {
						return DefragCountConflict;
				  }
			  }

			  public override void Delete()
			  { // Nothing to delete
			  }
		 }
	}

}