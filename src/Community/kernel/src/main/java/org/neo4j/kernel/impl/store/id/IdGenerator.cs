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
namespace Neo4Net.Kernel.impl.store.id
{

	public interface IdGenerator : IdSequence, System.IDisposable
	{
		 IdRange NextIdBatch( int size );

		 /// <param name="id"> the highest in use + 1 </param>
		 long HighId { set;get; }
		 long HighestPossibleIdInUse { get; }
		 void FreeId( long id );

		 /// <summary>
		 /// Closes the id generator, marking it as clean.
		 /// </summary>
		 void Close();
		 long NumberOfIdsInUse { get; }
		 long DefragCount { get; }

		 /// <summary>
		 /// Closes the id generator as dirty and deletes it right after closed. This operation is safe, in the sense
		 /// that the id generator file is closed but not marked as clean. This has the net result that a crash in the
		 /// middle will still leave the file marked as dirty so it will be deleted on the next open call.
		 /// </summary>
		 void Delete();
	}

	 public class IdGenerator_Delegate : IdGenerator
	 {
		  internal readonly IdGenerator Delegate;

		  public IdGenerator_Delegate( IdGenerator @delegate )
		  {
				this.Delegate = @delegate;
		  }

		  public override long NextId()
		  {
				return Delegate.nextId();
		  }

		  public override IdRange NextIdBatch( int size )
		  {
				return Delegate.nextIdBatch( size );
		  }

		  public virtual long HighId
		  {
			  set
			  {
					Delegate.HighId = value;
			  }
			  get
			  {
					return Delegate.HighId;
			  }
		  }


		  public virtual long HighestPossibleIdInUse
		  {
			  get
			  {
					return Delegate.HighestPossibleIdInUse;
			  }
		  }

		  public override void FreeId( long id )
		  {
				Delegate.freeId( id );
		  }

		  public override void Close()
		  {
				Delegate.Dispose();
		  }

		  public virtual long NumberOfIdsInUse
		  {
			  get
			  {
					return Delegate.NumberOfIdsInUse;
			  }
		  }

		  public virtual long DefragCount
		  {
			  get
			  {
					return Delegate.DefragCount;
			  }
		  }

		  public override void Delete()
		  {
				Delegate.delete();
		  }
	 }

}