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
namespace Neo4Net.Kernel.impl.core
{

	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;
	using EphemeralIdGenerator = Neo4Net.Test.impl.EphemeralIdGenerator;

	public class JumpingIdGeneratorFactory : IdGeneratorFactory
	{
		 private readonly IDictionary<IdType, IdGenerator> _generators = new Dictionary<IdType, IdGenerator>( typeof( IdType ) );
		 private readonly IdGenerator _forTheRest = new EphemeralIdGenerator( null, null );

		 private readonly int _sizePerJump;

		 public JumpingIdGeneratorFactory( int sizePerJump )
		 {
			  this._sizePerJump = sizePerJump;
		 }

		 public override IdGenerator Open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  return Get( idType );
		 }

		 public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
		 {
			  return Get( idType );
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  if ( idType == IdType.NODE || idType == IdType.RELATIONSHIP || idType == IdType.PROPERTY || idType == IdType.STRING_BLOCK || idType == IdType.ARRAY_BLOCK )
			  {
					return _generators.computeIfAbsent( idType, k => new JumpingIdGenerator( this ) );
			  }
			  return _forTheRest;
		 }

		 public override void Create( File fileName, long highId, bool throwIfFileExists )
		 {
		 }

		 private class JumpingIdGenerator : IdGenerator
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 LeftToNextJump = outerInstance.sizePerJump / 2;
			 }

			 private readonly JumpingIdGeneratorFactory _outerInstance;

			 public JumpingIdGenerator( JumpingIdGeneratorFactory outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicLong NextIdConflict = new AtomicLong();
			  internal int LeftToNextJump;
			  internal long HighBits;

			  public override long NextId()
			  {
					long result = TryNextId();
					if ( --LeftToNextJump == 0 )
					{
						 LeftToNextJump = outerInstance.sizePerJump;
						 NextIdConflict.set( ( 0xFFFFFFFFL | ( HighBits++ << 32 ) ) - outerInstance.sizePerJump / 2 + 1 );
					}
					return result;
			  }

			  internal virtual long TryNextId()
			  {
					long result = NextIdConflict.AndIncrement;
					if ( IdValidator.isReservedId( result ) )
					{
						 result = NextIdConflict.AndIncrement;
						 LeftToNextJump--;
					}
					return result;
			  }

			  public override IdRange NextIdBatch( int size )
			  {
					throw new System.NotSupportedException();
			  }

			  public virtual long HighId
			  {
				  get
				  {
						return NextIdConflict.get();
				  }
				  set
				  {
						NextIdConflict.set( value );
				  }
			  }


			  public override void FreeId( long id )
			  {
			  }

			  public override void Close()
			  {
			  }

			  public virtual long NumberOfIdsInUse
			  {
				  get
				  {
						return NextIdConflict.get();
				  }
			  }

			  public virtual long DefragCount
			  {
				  get
				  {
						return 0;
				  }
			  }

			  public override void Delete()
			  {
			  }

			  public virtual long HighestPossibleIdInUse
			  {
				  get
				  {
						return HighId - 1;
				  }
			  }
		 }
	}

}