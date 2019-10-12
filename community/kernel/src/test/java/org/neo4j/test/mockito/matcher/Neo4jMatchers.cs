using System.Collections.Generic;
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
namespace Org.Neo4j.Test.mockito.matcher
{
	using Description = org.hamcrest.Description;
	using DiagnosingMatcher = org.hamcrest.DiagnosingMatcher;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeDiagnosingMatcher = org.hamcrest.TypeSafeDiagnosingMatcher;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexCreator = Org.Neo4j.Graphdb.schema.IndexCreator;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Schema = Org.Neo4j.Graphdb.schema.Schema;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.loop;

	public class Neo4jMatchers
	{
		 private Neo4jMatchers()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T> org.hamcrest.Matcher<? super T> inTx(final org.neo4j.graphdb.GraphDatabaseService db, final org.hamcrest.Matcher<T> inner)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> InTx<T>( GraphDatabaseService db, Matcher<T> inner )
		 {
			  return InTx( db, inner, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T> org.hamcrest.Matcher<? super T> inTx(final org.neo4j.graphdb.GraphDatabaseService db, final org.hamcrest.Matcher<T> inner, final boolean successful)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> InTx<T>( GraphDatabaseService db, Matcher<T> inner, bool successful )
		 {
			  return new DiagnosingMatcherAnonymousInnerClass( db, inner, successful );
		 }

		 private class DiagnosingMatcherAnonymousInnerClass : DiagnosingMatcher<T>
		 {
			 private GraphDatabaseService _db;
			 private Matcher<T> _inner;
			 private bool _successful;

			 public DiagnosingMatcherAnonymousInnerClass( GraphDatabaseService db, Matcher<T> inner, bool successful )
			 {
				 this._db = db;
				 this._inner = inner;
				 this._successful = successful;
			 }

			 protected internal override bool matches( object item, Description mismatchDescription )
			 {
				  using ( Transaction ignored = _db.beginTx() )
				  {
						if ( _inner.matches( item ) )
						{
							 if ( _successful )
							 {
								  ignored.Success();
							 }
							 return true;
						}

						_inner.describeMismatch( item, mismatchDescription );

						if ( _successful )
						{
							 ignored.Success();
						}
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  _inner.describeTo( description );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<org.neo4j.graphdb.Node> hasLabel(final org.neo4j.graphdb.Label myLabel)
		 public static TypeSafeDiagnosingMatcher<Node> HasLabel( Label myLabel )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass( myLabel );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass : TypeSafeDiagnosingMatcher<Node>
		 {
			 private Label _myLabel;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass( Label myLabel )
			 {
				 this._myLabel = myLabel;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( _myLabel );
			 }

			 protected internal override bool matchesSafely( Node item, Description mismatchDescription )
			 {
				  bool result = item.HasLabel( _myLabel );
				  if ( !result )
				  {
						ISet<string> labels = AsLabelNameSet( item.Labels );
						mismatchDescription.appendText( labels.ToString() );
				  }
				  return result;
			 }
		 }

		 public static TypeSafeDiagnosingMatcher<Node> HasLabels( params string[] expectedLabels )
		 {
			  return HasLabels( asSet( expectedLabels ) );
		 }

		 public static TypeSafeDiagnosingMatcher<Node> HasLabels( params Label[] expectedLabels )
		 {
			  ISet<string> labelNames = new HashSet<string>( expectedLabels.Length );
			  foreach ( Label l in expectedLabels )
			  {
					labelNames.Add( l.Name() );
			  }
			  return HasLabels( labelNames );
		 }

		 public static TypeSafeDiagnosingMatcher<Node> HasNoLabels()
		 {
			  return HasLabels( emptySet() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<org.neo4j.graphdb.Node> hasLabels(final java.util.Set<String> expectedLabels)
		 public static TypeSafeDiagnosingMatcher<Node> HasLabels( ISet<string> expectedLabels )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass2( expectedLabels );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass2 : TypeSafeDiagnosingMatcher<Node>
		 {
			 private ISet<string> _expectedLabels;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass2( ISet<string> expectedLabels )
			 {
				 this._expectedLabels = expectedLabels;
			 }

			 private ISet<string> foundLabels;

			 public override void describeTo( Description description )
			 {
				  description.appendText( _expectedLabels.ToString() );
			 }

			 protected internal override bool matchesSafely( Node item, Description mismatchDescription )
			 {
				  foundLabels = AsLabelNameSet( item.Labels );

				  if ( foundLabels.size() == _expectedLabels.Count && foundLabels.containsAll(_expectedLabels) )
				  {
						return true;
				  }

				  mismatchDescription.appendText( "was " + foundLabels.ToString() );
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<org.neo4j.graphdb.GraphDatabaseService> hasNoNodes(final org.neo4j.graphdb.Label withLabel)
		 public static TypeSafeDiagnosingMatcher<GraphDatabaseService> HasNoNodes( Label withLabel )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass3( withLabel );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass3 : TypeSafeDiagnosingMatcher<GraphDatabaseService>
		 {
			 private Label _withLabel;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass3( Label withLabel )
			 {
				 this._withLabel = withLabel;
			 }

			 protected internal override bool matchesSafely( GraphDatabaseService db, Description mismatchDescription )
			 {
				  ISet<Node> found = asSet( Db.findNodes( _withLabel ) );
				  if ( found.Count > 0 )
				  {
						mismatchDescription.appendText( "found " + found.ToString() );
						return false;
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "no nodes with label " + _withLabel );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<org.neo4j.graphdb.GraphDatabaseService> hasNodes(final org.neo4j.graphdb.Label withLabel, final org.neo4j.graphdb.Node... expectedNodes)
		 public static TypeSafeDiagnosingMatcher<GraphDatabaseService> HasNodes( Label withLabel, params Node[] expectedNodes )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass4( withLabel, expectedNodes );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass4 : TypeSafeDiagnosingMatcher<GraphDatabaseService>
		 {
			 private Label _withLabel;
			 private Node[] _expectedNodes;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass4( Label withLabel, Node[] expectedNodes )
			 {
				 this._withLabel = withLabel;
				 this._expectedNodes = expectedNodes;
			 }

			 protected internal override bool matchesSafely( GraphDatabaseService db, Description mismatchDescription )
			 {
				  ISet<Node> expected = asSet( _expectedNodes );
				  ISet<Node> found = asSet( Db.findNodes( _withLabel ) );
				  if ( !expected.SetEquals( found ) )
				  {
						mismatchDescription.appendText( "found " + found.ToString() );
						return false;
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( asSet( _expectedNodes ).ToString() + " with label " + _withLabel );
			 }
		 }

		 public static ISet<string> AsLabelNameSet( IEnumerable<Label> enums )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Iterables.asSet( map( Label::name, enums ) );
		 }

		 public class PropertyValueMatcher : TypeSafeDiagnosingMatcher<PropertyContainer>
		 {
			  internal readonly PropertyMatcher PropertyMatcher;
			  internal readonly string PropertyName;
			  internal readonly object ExpectedValue;

			  internal PropertyValueMatcher( PropertyMatcher propertyMatcher, string propertyName, object expectedValue )
			  {
					this.PropertyMatcher = propertyMatcher;
					this.PropertyName = propertyName;
					this.ExpectedValue = expectedValue;
			  }

			  protected internal override bool MatchesSafely( PropertyContainer propertyContainer, Description mismatchDescription )
			  {
					if ( !PropertyMatcher.matchesSafely( propertyContainer, mismatchDescription ) )
					{
						 return false;
					}

					object foundValue = propertyContainer.GetProperty( PropertyName );
					if ( !PropertyValuesEqual( ExpectedValue, foundValue ) )
					{
						 mismatchDescription.appendText( "found value " + FormatValue( foundValue ) );
						 return false;
					}
					return true;
			  }

			  public override void DescribeTo( Description description )
			  {
					PropertyMatcher.describeTo( description );
					description.appendText( string.Format( "having value {0}", FormatValue( ExpectedValue ) ) );
			  }

			  internal virtual bool PropertyValuesEqual( object expected, object readValue )
			  {
					if ( expected.GetType().IsArray )
					{
						 return ArrayAsCollection( expected ).Equals( ArrayAsCollection( readValue ) );
					}
					return expected.Equals( readValue );
			  }

			  internal virtual string FormatValue( object v )
			  {
					if ( v is string )
					{
						 return string.Format( "'{0}'", v.ToString() );
					}
					return v.ToString();
			  }

		 }

		 public class PropertyMatcher : TypeSafeDiagnosingMatcher<PropertyContainer>
		 {

			  public readonly string PropertyName;

			  internal PropertyMatcher( string propertyName )
			  {
					this.PropertyName = propertyName;
			  }

			  protected internal override bool MatchesSafely( PropertyContainer propertyContainer, Description mismatchDescription )
			  {
					if ( !propertyContainer.HasProperty( PropertyName ) )
					{
						 mismatchDescription.appendText( string.Format( "found property container with property keys: {0}", Iterables.asSet( propertyContainer.PropertyKeys ) ) );
						 return false;
					}
					return true;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( string.Format( "property container with property name '{0}' ", PropertyName ) );
			  }

			  public virtual PropertyValueMatcher WithValue( object value )
			  {
					return new PropertyValueMatcher( this, PropertyName, value );
			  }
		 }

		 public static PropertyMatcher HasProperty( string propertyName )
		 {
			  return new PropertyMatcher( propertyName );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Deferred<org.neo4j.graphdb.Node> findNodesByLabelAndProperty(final org.neo4j.graphdb.Label label, final String propertyName, final Object propertyValue, final org.neo4j.graphdb.GraphDatabaseService db)
		 public static Deferred<Node> FindNodesByLabelAndProperty( Label label, string propertyName, object propertyValue, GraphDatabaseService db )
		 {
			  return new DeferredAnonymousInnerClass( db, label, propertyName, propertyValue );
		 }

		 private class DeferredAnonymousInnerClass : Deferred<Node>
		 {
			 private Label _label;
			 private string _propertyName;
			 private object _propertyValue;
			 private GraphDatabaseService _db;

			 public DeferredAnonymousInnerClass( UnknownType db, Label label, string propertyName, object propertyValue, GraphDatabaseService db ) : base( db )
			 {
				 this._label = label;
				 this._propertyName = propertyName;
				 this._propertyValue = propertyValue;
				 this._db = db;
			 }

			 protected internal override IEnumerable<Node> manifest()
			 {
				  return loop( _db.findNodes( _label, _propertyName, _propertyValue ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Deferred<org.neo4j.graphdb.schema.IndexDefinition> getIndexes(final org.neo4j.graphdb.GraphDatabaseService db, final org.neo4j.graphdb.Label label)
		 public static Deferred<IndexDefinition> GetIndexes( GraphDatabaseService db, Label label )
		 {
			  return new DeferredAnonymousInnerClass2( db, label );
		 }

		 private class DeferredAnonymousInnerClass2 : Deferred<IndexDefinition>
		 {
			 private GraphDatabaseService _db;
			 private Label _label;

			 public DeferredAnonymousInnerClass2( UnknownType db, GraphDatabaseService db, Label label ) : base( db )
			 {
				 this._db = db;
				 this._label = label;
			 }

			 protected internal override IEnumerable<IndexDefinition> manifest()
			 {
				  return _db.schema().getIndexes(_label);
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Deferred<String> getPropertyKeys(final org.neo4j.graphdb.GraphDatabaseService db, final org.neo4j.graphdb.PropertyContainer propertyContainer)
		 public static Deferred<string> GetPropertyKeys( GraphDatabaseService db, PropertyContainer propertyContainer )
		 {
			  return new DeferredAnonymousInnerClass3( db, propertyContainer );
		 }

		 private class DeferredAnonymousInnerClass3 : Deferred<string>
		 {
			 private PropertyContainer _propertyContainer;

			 public DeferredAnonymousInnerClass3( UnknownType db, PropertyContainer propertyContainer ) : base( db )
			 {
				 this._propertyContainer = propertyContainer;
			 }

			 protected internal override IEnumerable<string> manifest()
			 {
				  return _propertyContainer.PropertyKeys;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Deferred<org.neo4j.graphdb.schema.ConstraintDefinition> getConstraints(final org.neo4j.graphdb.GraphDatabaseService db, final org.neo4j.graphdb.Label label)
		 public static Deferred<ConstraintDefinition> GetConstraints( GraphDatabaseService db, Label label )
		 {
			  return new DeferredAnonymousInnerClass4( db, label );
		 }

		 private class DeferredAnonymousInnerClass4 : Deferred<ConstraintDefinition>
		 {
			 private GraphDatabaseService _db;
			 private Label _label;

			 public DeferredAnonymousInnerClass4( UnknownType db, GraphDatabaseService db, Label label ) : base( db )
			 {
				 this._db = db;
				 this._label = label;
			 }

			 protected internal override IEnumerable<ConstraintDefinition> manifest()
			 {
				  return _db.schema().getConstraints(_label);
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Deferred<org.neo4j.graphdb.schema.ConstraintDefinition> getConstraints(final org.neo4j.graphdb.GraphDatabaseService db, final org.neo4j.graphdb.RelationshipType type)
		 public static Deferred<ConstraintDefinition> GetConstraints( GraphDatabaseService db, RelationshipType type )
		 {
			  return new DeferredAnonymousInnerClass5( db, type );
		 }

		 private class DeferredAnonymousInnerClass5 : Deferred<ConstraintDefinition>
		 {
			 private GraphDatabaseService _db;
			 private RelationshipType _type;

			 public DeferredAnonymousInnerClass5( UnknownType db, GraphDatabaseService db, RelationshipType type ) : base( db )
			 {
				 this._db = db;
				 this._type = type;
			 }

			 protected internal override IEnumerable<ConstraintDefinition> manifest()
			 {
				  return _db.schema().getConstraints(_type);
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Deferred<org.neo4j.graphdb.schema.ConstraintDefinition> getConstraints(final org.neo4j.graphdb.GraphDatabaseService db)
		 public static Deferred<ConstraintDefinition> GetConstraints( GraphDatabaseService db )
		 {
			  return new DeferredAnonymousInnerClass6( db );
		 }

		 private class DeferredAnonymousInnerClass6 : Deferred<ConstraintDefinition>
		 {
			 private GraphDatabaseService _db;

			 public DeferredAnonymousInnerClass6( UnknownType db, GraphDatabaseService db ) : base( db )
			 {
				 this._db = db;
			 }

			 protected internal override IEnumerable<ConstraintDefinition> manifest()
			 {
				  return _db.schema().Constraints;
			 }
		 }

		 /// <summary>
		 /// Represents test data that can at assertion time produce a collection
		 /// 
		 /// Useful to defer actually doing operations until context has been prepared (such as a transaction created)
		 /// </summary>
		 /// @param <T> The type of objects the collection will contain </param>
		 public abstract class Deferred<T>
		 {

			  internal readonly GraphDatabaseService Db;

			  public Deferred( GraphDatabaseService db )
			  {
					this.Db = db;
			  }

			  protected internal abstract IEnumerable<T> Manifest();

			  public virtual ICollection<T> Collection()
			  {
					using ( Transaction ignore = Db.beginTx() )
					{
						 return Iterables.asCollection( Manifest() );
					}
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> org.hamcrest.TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<T>> containsOnly(final T... expectedObjects)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<T>> ContainsOnly<T>( params T[] expectedObjects )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass( expectedObjects );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass : TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<T>>
		 {
			 private T[] _expectedObjects;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass( T[] expectedObjects )
			 {
				 this._expectedObjects = expectedObjects;
			 }

			 protected internal override bool matchesSafely( Neo4jMatchers.Deferred<T> nodes, Description description )
			 {
				  ISet<T> expected = asSet( _expectedObjects );
				  ISet<T> found = Iterables.asSet( nodes.Collection() );
				  if ( !expected.SetEquals( found ) )
				  {
						description.appendText( "found " + found.ToString() );
						return false;
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "exactly " + asSet( _expectedObjects ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<?>> hasSize(final int expectedSize)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<object>> HasSize( int expectedSize )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new org.hamcrest.TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<?>>()
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass2( expectedSize );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass2 : TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<JavaToDotNetGenericWildcard>>
		 {
			 private int _expectedSize;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass2( int expectedSize )
			 {
				 this._expectedSize = expectedSize;
			 }

			 protected internal override bool matchesSafely<T1>( Neo4jMatchers.Deferred<T1> nodes, Description description )
			 {
				  int foundSize = nodes.Collection().Count;

				  if ( foundSize != _expectedSize )
				  {
						description.appendText( "found " + nodes.Collection().ToString() );
						return false;
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "collection of size " + _expectedSize );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<org.neo4j.graphdb.schema.IndexDefinition>> haveState(final org.neo4j.graphdb.GraphDatabaseService db, final org.neo4j.graphdb.schema.Schema_IndexState expectedState)
		 public static TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<IndexDefinition>> HaveState( GraphDatabaseService db, Org.Neo4j.Graphdb.schema.Schema_IndexState expectedState )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass3( db, expectedState );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass3 : TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<IndexDefinition>>
		 {
			 private GraphDatabaseService _db;
			 private Org.Neo4j.Graphdb.schema.Schema_IndexState _expectedState;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass3( GraphDatabaseService db, Org.Neo4j.Graphdb.schema.Schema_IndexState expectedState )
			 {
				 this._db = db;
				 this._expectedState = expectedState;
			 }

			 protected internal override bool matchesSafely( Neo4jMatchers.Deferred<IndexDefinition> indexes, Description description )
			 {
				  foreach ( IndexDefinition current in indexes.Collection() )
				  {
						Org.Neo4j.Graphdb.schema.Schema_IndexState currentState = _db.schema().getIndexState(current);
						if ( !currentState.Equals( _expectedState ) )
						{
							 description.appendValue( current ).appendText( " has state " ).appendValue( currentState );
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "all indexes have state " + _expectedState );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> org.hamcrest.TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<T>> contains(final T... expectedObjects)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<T>> Contains<T>( params T[] expectedObjects )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass4( expectedObjects );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass4 : TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<T>>
		 {
			 private T[] _expectedObjects;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass4( T[] expectedObjects )
			 {
				 this._expectedObjects = expectedObjects;
			 }

			 protected internal override bool matchesSafely( Neo4jMatchers.Deferred<T> nodes, Description description )
			 {
				  ISet<T> expected = asSet( _expectedObjects );
				  ISet<T> found = Iterables.asSet( nodes.Collection() );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
				  if ( !found.containsAll( expected ) )
				  {
						description.appendText( "found " + found.ToString() );
						return false;
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "contains " + asSet( _expectedObjects ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static org.hamcrest.TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<?>> isEmpty()
		 public static TypeSafeDiagnosingMatcher<Neo4jMatchers.Deferred<object>> Empty
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: return new org.hamcrest.TypeSafeDiagnosingMatcher<Deferred<?>>()
				  return new TypeSafeDiagnosingMatcherAnonymousInnerClass5();
			 }
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass5 : TypeSafeDiagnosingMatcher<Deferred<JavaToDotNetGenericWildcard>>
		 {
			 protected internal override bool matchesSafely<T1>( Deferred<T1> deferred, Description description )
			 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<?> collection = deferred.collection();
				  ICollection<object> collection = deferred.Collection();
				  if ( collection.Count > 0 )
				  {
						description.appendText( "was " + collection.ToString() );
						return false;
				  }

				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "empty collection" );
			 }
		 }

		 public static IndexDefinition CreateIndex( GraphDatabaseService beansAPI, Label label, params string[] properties )
		 {
			  IndexDefinition indexDef = CreateIndexNoWait( beansAPI, label, properties );

			  WaitForIndex( beansAPI, indexDef );
			  return indexDef;
		 }

		 public static IndexDefinition CreateIndexNoWait( GraphDatabaseService beansAPI, Label label, params string[] properties )
		 {
			  IndexDefinition indexDef;
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					IndexCreator indexCreator = beansAPI.Schema().indexFor(label);
					foreach ( string property in properties )
					{
						 indexCreator = indexCreator.On( property );
					}
					indexDef = indexCreator.Create();
					tx.Success();
			  }
			  return indexDef;
		 }

		 public static void WaitForIndex( GraphDatabaseService beansAPI, IndexDefinition indexDef )
		 {
			  using ( Transaction ignored = beansAPI.BeginTx() )
			  {
					beansAPI.Schema().awaitIndexOnline(indexDef, 30, SECONDS);
			  }
		 }

		 public static void WaitForIndexes( GraphDatabaseService beansAPI )
		 {
			  using ( Transaction ignored = beansAPI.BeginTx() )
			  {
					beansAPI.Schema().awaitIndexesOnline(30, SECONDS);
			  }
		 }

		 public static object GetIndexState( GraphDatabaseService beansAPI, IndexDefinition indexDef )
		 {
			  using ( Transaction ignored = beansAPI.BeginTx() )
			  {
					return beansAPI.Schema().getIndexState(indexDef);
			  }
		 }

		 public static ConstraintDefinition CreateConstraint( GraphDatabaseService db, Label label, string propertyKey )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					ConstraintDefinition constraint = Db.schema().constraintFor(label).assertPropertyIsUnique(propertyKey).create();
					tx.Success();
					return constraint;
			  }
		 }

		 public static ICollection<object> ArrayAsCollection( object arrayValue )
		 {
			  Debug.Assert( arrayValue.GetType().IsArray );

			  ICollection<object> result = new List<object>();
			  int length = Array.getLength( arrayValue );
			  for ( int i = 0; i < length; i++ )
			  {
					result.Add( Array.get( arrayValue, i ) );
			  }
			  return result;
		 }
	}

}