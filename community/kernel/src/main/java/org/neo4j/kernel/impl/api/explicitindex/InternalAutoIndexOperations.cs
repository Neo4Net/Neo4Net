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
namespace Org.Neo4j.Kernel.Impl.Api.explicitindex
{

	using ExplicitIndexWrite = Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using PropertyKeyIdNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using AutoIndexingKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using AutoIndexOperations = Org.Neo4j.Kernel.api.explicitindex.AutoIndexOperations;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenNotFoundException = Org.Neo4j.Kernel.impl.core.TokenNotFoundException;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class InternalAutoIndexOperations : AutoIndexOperations
	{
		 public abstract class EntityType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODE { public void add(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException { ops.nodeAddToExplicitIndex(InternalAutoIndexing.NODE_AUTO_INDEX, entityId, keyName, value); } public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException { ops.nodeRemoveFromExplicitIndex(InternalAutoIndexing.NODE_AUTO_INDEX, entityId, keyName, value); } public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException { ops.nodeRemoveFromExplicitIndex(InternalAutoIndexing.NODE_AUTO_INDEX, entityId, keyName); } public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException { ops.nodeRemoveFromExplicitIndex(InternalAutoIndexing.NODE_AUTO_INDEX, entityId); } public void ensureIndexExists(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops) { ops.nodeExplicitIndexCreateLazily(InternalAutoIndexing.NODE_AUTO_INDEX, null); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIP { public void add(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException { ops.relationshipAddToExplicitIndex(InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX, entityId, keyName, value); } public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException { ops.relationshipRemoveFromExplicitIndex(InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX, entityId, keyName, value); } public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName) throws org.neo4j.internal.kernel.api.exceptions.KernelException { ops.relationshipRemoveFromExplicitIndex(InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX, entityId, keyName); } public void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId) throws org.neo4j.internal.kernel.api.exceptions.KernelException { ops.relationshipRemoveFromExplicitIndex(InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX, entityId); } public void ensureIndexExists(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops) { ops.relationshipExplicitIndexCreateLazily(InternalAutoIndexing.RELATIONSHIP_AUTO_INDEX, null); } };

			  private static readonly IList<EntityType> valueList = new List<EntityType>();

			  static EntityType()
			  {
				  valueList.Add( NODE );
				  valueList.Add( RELATIONSHIP );
			  }

			  public enum InnerEnum
			  {
				  NODE,
				  RELATIONSHIP
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private EntityType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void add(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  public abstract void add( Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite ops, long entityId, string keyName, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  public abstract void remove( Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite ops, long entityId, string keyName, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, String keyName) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  public abstract void remove( Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite ops, long entityId, string keyName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void remove(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  public abstract void remove( Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite ops, long entityId );

			  public abstract void ensureIndexExists( Org.Neo4j.@internal.Kernel.Api.ExplicitIndexWrite write );

			 public static IList<EntityType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static EntityType valueOf( string name )
			 {
				 foreach ( EntityType enumInstance in EntityType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private AtomicReference<ISet<string>> _propertyKeysToInclude = new AtomicReference<ISet<string>>( Collections.emptySet() );

		 private readonly TokenHolder _propertyKeyLookup;
		 private readonly EntityType _type;

		 private volatile bool _enabled;
		 private volatile bool _indexCreated;

		 public InternalAutoIndexOperations( TokenHolder propertyKeyLookup, EntityType type )
		 {
			  this._propertyKeyLookup = propertyKeyLookup;
			  this._type = type;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void propertyAdded(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, int propertyKeyId, org.neo4j.values.storable.Value value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override void PropertyAdded( ExplicitIndexWrite ops, long entityId, int propertyKeyId, Value value )
		 {
			  if ( _enabled )
			  {
					try
					{
						 string name = _propertyKeyLookup.getTokenById( propertyKeyId ).name();
						 if ( _propertyKeysToInclude.get().contains(name) )
						 {
							  EnsureIndexExists( ops );
							  _type.add( ops, entityId, name, value.AsObject() );
						 }
					}
					catch ( KernelException e )
					{
						 throw new AutoIndexingKernelException( e );
					}
					catch ( TokenNotFoundException e )
					{
						 // TODO: TokenNotFoundException was added before there was a kernel. It should be converted to a
						 // KernelException now
						 throw new AutoIndexingKernelException( new PropertyKeyIdNotFoundKernelException( propertyKeyId, e ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void propertyChanged(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, int propertyKeyId, org.neo4j.values.storable.Value oldValue, org.neo4j.values.storable.Value newValue) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override void PropertyChanged( ExplicitIndexWrite ops, long entityId, int propertyKeyId, Value oldValue, Value newValue )
		 {
			  if ( _enabled )
			  {
					try
					{
						 string name = _propertyKeyLookup.getTokenById( propertyKeyId ).name();
						 if ( _propertyKeysToInclude.get().contains(name) )
						 {
							  EnsureIndexExists( ops );
							  _type.remove( ops, entityId, name, oldValue.AsObject() );
							  _type.add( ops, entityId, name, newValue.AsObject() );
						 }
					}
					catch ( KernelException e )
					{
						 throw new AutoIndexingKernelException( e );
					}
					catch ( TokenNotFoundException e )
					{
						 // TODO: TokenNotFoundException was added before there was a kernel. It should be converted to a
						 // KernelException now
						 throw new AutoIndexingKernelException( new PropertyKeyIdNotFoundKernelException( propertyKeyId, e ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void propertyRemoved(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId, int propertyKey) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override void PropertyRemoved( ExplicitIndexWrite ops, long entityId, int propertyKey )
		 {
			  if ( _enabled )
			  {
					try
					{
						 string name = _propertyKeyLookup.getTokenById( propertyKey ).name();
						 if ( _propertyKeysToInclude.get().contains(name) )
						 {
							  EnsureIndexExists( ops );
							  _type.remove( ops, entityId, name );
						 }
					}
					catch ( KernelException e )
					{
						 throw new AutoIndexingKernelException( e );
					}
					catch ( TokenNotFoundException e )
					{
						 // TODO: TokenNotFoundException was added before there was a kernel. It should be converted to a
						 // KernelException now
						 throw new AutoIndexingKernelException( new PropertyKeyIdNotFoundKernelException( propertyKey, e ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void entityRemoved(org.neo4j.internal.kernel.api.ExplicitIndexWrite ops, long entityId) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
		 public override void EntityRemoved( ExplicitIndexWrite ops, long entityId )
		 {
			  if ( _enabled )
			  {
					try
					{
						 EnsureIndexExists( ops );
						 _type.remove( ops, entityId );
					}
					catch ( KernelException e )
					{
						 throw new AutoIndexingKernelException( e );
					}
			  }
		 }

		 // Trap door needed to keep this as an enum
		 internal virtual void ReplacePropertyKeysToInclude( IList<string> propertyKeysToIncludeNow )
		 {
			  ISet<string> copiedPropertyKeysToIncludeNow = new HashSet<string>( propertyKeysToIncludeNow.Count );
			  copiedPropertyKeysToIncludeNow.addAll( propertyKeysToIncludeNow );
			  this._propertyKeysToInclude.set( copiedPropertyKeysToIncludeNow );
		 }

		 public override void Enabled( bool enabled )
		 {
			  this._enabled = enabled;
		 }

		 public override bool Enabled()
		 {
			  return _enabled;
		 }

		 public override void StartAutoIndexingProperty( string propName )
		 {
			  _propertyKeysToInclude.getAndUpdate(current =>
			  {
				ISet<string> updated = new HashSet<string>();
				updated.addAll( current );
				updated.add( propName );
				return updated;
			  });
		 }

		 public override void StopAutoIndexingProperty( string propName )
		 {
			  _propertyKeysToInclude.getAndUpdate(current =>
			  {
				ISet<string> updated = new HashSet<string>();
				updated.addAll( current );
				updated.remove( propName );
				return updated;
			  });
		 }

		 public virtual ISet<string> AutoIndexedProperties
		 {
			 get
			 {
				  return Collections.unmodifiableSet( _propertyKeysToInclude.get() );
			 }
		 }

		 private void EnsureIndexExists( ExplicitIndexWrite ops )
		 {
			  // Known racy, but this is safe because ensureIndexExists is concurrency safe, we just want to avoid calling it
			  // for every single write we make.
			  if ( !_indexCreated )
			  {
					_type.ensureIndexExists( ops );
					_indexCreated = true;
			  }
		 }
	}

}