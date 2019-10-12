using System;
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
namespace Neo4Net.Kernel.ha.id
{

	using ComException = Neo4Net.com.ComException;
	using Neo4Net.com;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Neo4Net.Kernel.ha;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdRangeIterator = Neo4Net.Kernel.impl.store.id.IdRangeIterator;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using IdTypeConfiguration = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.id.IdRangeIterator.EMPTY_ID_RANGE_ITERATOR;

	public class HaIdGeneratorFactory : IdGeneratorFactory
	{
		 private readonly IDictionary<IdType, HaIdGenerator> _generators = new Dictionary<IdType, HaIdGenerator>( typeof( IdType ) );
		 private readonly FileSystemAbstraction _fs;
		 private readonly IdTypeConfigurationProvider _idTypeConfigurationProvider;
		 private readonly IdGeneratorFactory _localFactory;
		 private readonly DelegateInvocationHandler<Master> _master;
		 private readonly Log _log;
		 private readonly RequestContextFactory _requestContextFactory;
		 private IdGeneratorState _globalState = IdGeneratorState.Pending;

		 public HaIdGeneratorFactory( DelegateInvocationHandler<Master> master, LogProvider logProvider, RequestContextFactory requestContextFactory, FileSystemAbstraction fs, IdTypeConfigurationProvider idTypeConfigurationProvider )
		 {
			  this._fs = fs;
			  this._idTypeConfigurationProvider = idTypeConfigurationProvider;
			  this._localFactory = new DefaultIdGeneratorFactory( fs, idTypeConfigurationProvider );
			  this._master = master;
			  this._log = logProvider.getLog( this.GetType() );
			  this._requestContextFactory = requestContextFactory;
		 }

		 public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdTypeConfiguration idTypeConfiguration = _idTypeConfigurationProvider.getIdTypeConfiguration( idType );
			  return Open( filename, idTypeConfiguration.GrabSize, idType, highId, maxId );
		 }

		 public override IdGenerator Open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  HaIdGenerator previous = _generators.Remove( idType );
			  if ( previous != null )
			  {
					previous.Dispose();
			  }

			  IdGenerator initialIdGenerator;
			  switch ( _globalState )
			  {
			  case Neo4Net.Kernel.ha.id.HaIdGeneratorFactory.IdGeneratorState.Master:
					initialIdGenerator = _localFactory.open( fileName, grabSize, idType, highId, maxId );
					break;
			  case Neo4Net.Kernel.ha.id.HaIdGeneratorFactory.IdGeneratorState.Slave:
					// Initially we may call switchToSlave() before calling open, so we need this additional
					// (and, you might say, hacky) call to delete the .id file here as well as in switchToSlave().
					_fs.deleteFile( fileName );
					initialIdGenerator = new SlaveIdGenerator( idType, highId(), _master.cement(), _log, _requestContextFactory );
					break;
			  default:
					throw new System.InvalidOperationException( _globalState.name() );
			  }
			  HaIdGenerator haIdGenerator = new HaIdGenerator( this, initialIdGenerator, fileName, grabSize, idType, _globalState, maxId );
			  _generators[idType] = haIdGenerator;
			  return haIdGenerator;
		 }

		 public override void Create( File fileName, long highId, bool throwIfFileExists )
		 {
			  _localFactory.create( fileName, highId, false );
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  return _generators[idType];
		 }

		 public virtual void SwitchToMaster()
		 {
			  _globalState = IdGeneratorState.Master;
			  foreach ( HaIdGenerator generator in _generators.Values )
			  {
					generator.SwitchToMaster();
			  }
		 }

		 public virtual void SwitchToSlave()
		 {
			  _globalState = IdGeneratorState.Slave;
			  foreach ( HaIdGenerator generator in _generators.Values )
			  {
					generator.SwitchToSlave( _master.cement() );
			  }
		 }

		 private enum IdGeneratorState
		 {
			  Pending,
			  Slave,
			  Master
		 }

		 private class HaIdGenerator : IdGenerator
		 {
			 private readonly HaIdGeneratorFactory _outerInstance;

			  internal volatile IdGenerator Delegate;
			  internal readonly File FileName;
			  internal readonly int GrabSize;
			  internal readonly IdType IdType;
			  internal volatile IdGeneratorState State;
			  internal readonly long MaxId;

			  internal HaIdGenerator( HaIdGeneratorFactory outerInstance, IdGenerator initialDelegate, File fileName, int grabSize, IdType idType, IdGeneratorState initialState, long maxId )
			  {
				  this._outerInstance = outerInstance;
					Delegate = initialDelegate;
					this.FileName = fileName;
					this.GrabSize = grabSize;
					this.IdType = idType;
					this.State = initialState;
					this.MaxId = maxId;
					outerInstance.log.Debug( "Instantiated HaIdGenerator for " + initialDelegate + " " + idType + ", " + initialState );
			  }

			  internal virtual void SwitchToSlave( Master master )
			  {
					string previousDelegate = Delegate.ToString();
					long highId = Delegate.HighId;
					// The .id file is open and marked DIRTY
					Delegate.delete();
					// The .id file underneath is now gone
					Delegate = new SlaveIdGenerator( IdType, highId, master, outerInstance.log, outerInstance.requestContextFactory );
					outerInstance.log.Debug( "Instantiated slave delegate " + Delegate + " of type " + IdType + " with highid " + highId + " uniquetempvar." );
					State = IdGeneratorState.Slave;
			  }

			  internal virtual void SwitchToMaster()
			  {
					if ( State == IdGeneratorState.Slave )
					{
						 string previousDelegate = Delegate.ToString();
						 long highId = Delegate.HighId;
						 Delegate.delete();

						 outerInstance.localFactory.Create( FileName, highId, false );
						 Delegate = outerInstance.localFactory.Open( FileName, GrabSize, IdType, () => highId, MaxId );
						 outerInstance.log.Debug( "Instantiated master delegate " + Delegate + " of type " + IdType + " with highid " + highId + " uniquetempvar." );
					}
					else
					{
						 outerInstance.log.Debug( "Keeps " + Delegate );
					}

					State = IdGeneratorState.Master;
			  }

			  public override string ToString()
			  {
					return Delegate.ToString();
			  }

			  public override sealed bool Equals( object other )
			  {
					return Delegate.Equals( other );
			  }

			  public override sealed int GetHashCode()
			  {
					return Delegate.GetHashCode();
			  }

			  public override long NextId()
			  {
					if ( State == IdGeneratorState.Pending )
					{
						 throw new System.InvalidOperationException( State.name() );
					}

					return Delegate.nextId();
			  }

			  public override IdRange NextIdBatch( int size )
			  {
					if ( State == IdGeneratorState.Pending )
					{
						 throw new System.InvalidOperationException( State.name() );
					}

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

		 private class SlaveIdGenerator : IdGenerator
		 {
			  internal volatile long HighestIdInUse;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile long DefragCountConflict;
			  internal volatile IdRangeIterator IdQueue;
			  internal readonly Master Master;
			  internal readonly IdType IdType;
			  internal readonly Log Log;
			  internal readonly RequestContextFactory RequestContextFactory;

			  internal SlaveIdGenerator( IdType idType, long highId, Master master, Log log, RequestContextFactory requestContextFactory )
			  {
					this.IdType = idType;
					this.HighestIdInUse = highId;
					this.Master = master;
					this.Log = log;
					this.RequestContextFactory = requestContextFactory;
					IdQueue = EMPTY_ID_RANGE_ITERATOR;
			  }

			  public override void Close()
			  {
			  }

			  public override void FreeId( long id )
			  {
			  }

			  public virtual long HighId
			  {
				  get
				  {
						return HighestIdInUse;
				  }
				  set
				  {
						this.HighestIdInUse = Math.Max( this.HighestIdInUse, value );
				  }
			  }

			  public virtual long HighestPossibleIdInUse
			  {
				  get
				  {
						return HighestIdInUse;
				  }
			  }

			  public virtual long NumberOfIdsInUse
			  {
				  get
				  {
						return HighestIdInUse - DefragCountConflict;
				  }
			  }

			  public override long NextId()
			  {
				  lock ( this )
				  {
						long nextId = NextLocalId();
						if ( nextId == IdRangeIterator.VALUE_REPRESENTING_NULL )
						{
							 AskForNextRangeFromMaster();
							 nextId = NextLocalId();
						}
						return nextId;
				  }
			  }

			  internal virtual void AskForNextRangeFromMaster()
			  {
					// If we don't have anymore grabbed ids from master, grab a bunch
					try
					{
							using ( Response<IdAllocation> response = Master.allocateIds( RequestContextFactory.newRequestContext(), IdType ) )
							{
							 IdAllocation allocation = response.ResponseConflict();
							 Log.info( "Received id allocation " + allocation + " from master " + Master + " for " + IdType );
							 StoreLocally( allocation );
							}
					}
					catch ( ComException e )
					{
						 throw new TransientTransactionFailureException( "Cannot allocate new entity ids from the cluster master. " + "The master instance is either down, or we have network connectivity problems", e );
					}
			  }

			  public override IdRange NextIdBatch( int size )
			  {
				  lock ( this )
				  {
						IdRange range = IdQueue.nextIdBatch( size );
						if ( range.TotalSize() == 0 )
						{
							 AskForNextRangeFromMaster();
							 range = IdQueue.nextIdBatch( size );
						}
						return range;
				  }
			  }

			  internal virtual void StoreLocally( IdAllocation allocation )
			  {
					HighId = allocation.HighestIdInUse;
					this.DefragCountConflict = allocation.DefragCount;
					this.IdQueue = allocation.IdRange.GetEnumerator();
			  }

			  internal virtual long NextLocalId()
			  {
					return this.IdQueue.nextId();
			  }


			  public virtual long DefragCount
			  {
				  get
				  {
						return this.DefragCountConflict;
				  }
			  }

			  public override void Delete()
			  {
			  }

			  public override string ToString()
			  {
					return this.GetType().Name + "[" + this.IdQueue + "]";
			  }
		 }
	}

}