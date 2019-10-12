using System.Diagnostics;

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

	using KernelTransactionsSnapshot = Org.Neo4j.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using IdTypeConfiguration = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfiguration;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;

	/// <summary>
	/// Wraps <seealso cref="IdGenerator"/> for those that have <seealso cref="IdTypeConfiguration.allowAggressiveReuse() aggressive id reuse"/>
	/// so that ids can be <seealso cref="IdGenerator.freeId(long) freed"/> at safe points in time, after all transactions
	/// which were active at the time of freeing, have been closed.
	/// </summary>
	public class BufferingIdGeneratorFactory : IdGeneratorFactory
	{
		 private readonly BufferingIdGenerator[] _overriddenIdGenerators = new BufferingIdGenerator[Enum.GetValues( typeof( IdType ) ).length];
		 private System.Func<KernelTransactionsSnapshot> _boundaries;
		 private readonly System.Predicate<KernelTransactionsSnapshot> _safeThreshold;
		 private readonly IdGeneratorFactory @delegate;
		 private readonly IdTypeConfigurationProvider _idTypeConfigurationProvider;

		 public BufferingIdGeneratorFactory( IdGeneratorFactory @delegate, IdReuseEligibility eligibleForReuse, IdTypeConfigurationProvider idTypeConfigurationProvider )
		 {
			  this.@delegate = @delegate;
			  this._idTypeConfigurationProvider = idTypeConfigurationProvider;
			  this._safeThreshold = snapshot => snapshot.allClosed() && eligibleForReuse.IsEligible(snapshot);
		 }

		 public virtual void Initialize( System.Func<KernelTransactionsSnapshot> transactionsSnapshotSupplier )
		 {
			  _boundaries = transactionsSnapshotSupplier;
		 }

		 public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
		 {
			  IdTypeConfiguration typeConfiguration = GetIdTypeConfiguration( idType );
			  return Open( filename, typeConfiguration.GrabSize, idType, highId, maxId );
		 }

		 public override IdGenerator Open( File filename, int grabSize, IdType idType, System.Func<long> highId, long maxId )
		 {
			  Debug.Assert( _boundaries != null, "Factory needs to be initialized before usage" );

			  IdGenerator generator = @delegate.Open( filename, grabSize, idType, highId, maxId );
			  IdTypeConfiguration typeConfiguration = GetIdTypeConfiguration( idType );
			  if ( typeConfiguration.AllowAggressiveReuse() )
			  {
					BufferingIdGenerator bufferingGenerator = new BufferingIdGenerator( generator );
					bufferingGenerator.Initialize( _boundaries, _safeThreshold );
					_overriddenIdGenerators[( int )idType] = bufferingGenerator;
					generator = bufferingGenerator;
			  }
			  return generator;
		 }

		 public override void Create( File filename, long highId, bool throwIfFileExists )
		 {
			  @delegate.Create( filename, highId, throwIfFileExists );
		 }

		 public override IdGenerator Get( IdType idType )
		 {
			  IdGenerator generator = _overriddenIdGenerators[( int )idType];
			  return generator != null ? generator : @delegate.Get( idType );
		 }

		 public virtual void Maintenance()
		 {
			  foreach ( BufferingIdGenerator generator in _overriddenIdGenerators )
			  {
					if ( generator != null )
					{
						 generator.Maintenance();
					}
			  }
		 }

		 public virtual void Clear()
		 {
			  foreach ( BufferingIdGenerator generator in _overriddenIdGenerators )
			  {
					if ( generator != null )
					{
						 generator.Clear();
					}
			  }
		 }

		 private IdTypeConfiguration GetIdTypeConfiguration( IdType idType )
		 {
			  return _idTypeConfigurationProvider.getIdTypeConfiguration( idType );
		 }
	}

}