using System;
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
namespace Neo4Net.Test
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.GraphDescription.PropType.ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.GraphDescription.PropType.STRING;

	public class GraphDescription : GraphDefinition
	{
		 [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
		 public class Graph : System.Attribute
		 {
			 private readonly GraphDescription _outerInstance;

			 public Graph;
			 {
			 }

			  internal string[] value;

			  internal NODE[] outerInstance.nodes;

			  internal REL[] relationships;

			  internal bool outerInstance.autoIndexNodes;

			  internal bool outerInstance.autoIndexRelationships;

			 public Graph( public Graph, String[] value = {}, NODE[] nodes = {}, REL[] relationships = {}, boolean autoIndexNodes = false, boolean autoIndexRelationships = false )
			 {
				 this.Graph = Graph;
				 this.value = value;
				 this.nodes = nodes;
				 this.relationships = relationships;
				 this.autoIndexNodes = autoIndexNodes;
				 this.autoIndexRelationships = autoIndexRelationships;
			 }
		 }

		 [AttributeUsage(, AllowMultiple = false, Inherited = false)]
		 public class NODE : System.Attribute
		 {
			 private readonly GraphDescription _outerInstance;

			 public NODE;
			 {
			 }

			  internal string name;

			  internal PROP[] properties;

			  internal LABEL[] labels;

			  internal bool setNameProperty;

			 public NODE( public NODE, String name, PROP[] properties = {}, LABEL[] labels = {}, boolean setNameProperty = false )
			 {
				 this.NODE = NODE;
				 this.name = name;
				 this.properties = properties;
				 this.labels = labels;
				 this.setNameProperty = setNameProperty;
			 }
		 }

		 [AttributeUsage(, AllowMultiple = false, Inherited = false)]
		 public class REL : System.Attribute
		 {
			 private readonly GraphDescription _outerInstance;

			 public REL;
			 {
			 }

			  internal string name;

			  internal string type;

			  internal string start;

			  internal string end;

			  internal PROP[] properties;

			  internal bool setNameProperty;

			 public REL( public REL, String type, String start, String end, String name = "", PROP[] properties = {}, boolean setNameProperty = false )
			 {
				 this.REL = REL;
				 this.name = name;
				 this.type = type;
				 this.start = start;
				 this.end = end;
				 this.properties = properties;
				 this.setNameProperty = setNameProperty;
			 }
		 }

		 [AttributeUsage(, AllowMultiple = false, Inherited = false)]
		 public class PROP : System.Attribute
		 {
			 private readonly GraphDescription _outerInstance;

			 public PROP;
			 {
			 }

			  internal string key;

			  internal string value;

			  internal PropType type;

			  internal PropType componentType;

			 public PROP( public PROP, String key, String value, PropType type = STRING, PropType componentType = ERROR )
			 {
				 this.PROP = PROP;
				 this.key = key;
				 this.value = value;
				 this.type = type;
				 this.componentType = componentType;
			 }
		 }

		 [AttributeUsage(, AllowMultiple = false, Inherited = false)]
		 public class LABEL : System.Attribute
		 {
			 private readonly GraphDescription _outerInstance;

			 public LABEL;
			 {
			 }

			  internal string value;

			 public LABEL( public LABEL, String value )
			 {
				 this.LABEL = LABEL;
				 this.value = value;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public enum PropType
		 public sealed class PropType
		 {

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ARRAY { Object convert(PropType componentType, String value) { String[] items = value.split(" *, *"); Object[] result = (Object[]) Array.newInstance(componentType.componentClass(), items.length); for(int i = 0; i < items.length; i++) { result[i] = componentType.convert(items[i]); } return result; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           STRING { String convert(String value) { return value; } Class componentClass() { return String.class; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INTEGER { Long convert(String value) { return long.Parse(value); } Class componentClass() { return Long.class; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           DOUBLE { Double convert(String value) { return double.Parse(value); } Class componentClass() { return Double.class; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           BOOLEAN { Boolean convert(String value) { return bool.Parse(value); } Class componentClass() { return Boolean.class; } },

//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ERROR { };

			  private static readonly IList<PropType> valueList = new List<PropType>();

			  static PropType()
			  {
				  valueList.Add( ARRAY );
				  valueList.Add( STRING );
				  valueList.Add( INTEGER );
				  valueList.Add( DOUBLE );
				  valueList.Add( BOOLEAN );
				  valueList.Add( ERROR );
			  }

			  public enum InnerEnum
			  {
				  ARRAY,
				  STRING,
				  INTEGER,
				  DOUBLE,
				  BOOLEAN,
				  ERROR
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private PropType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal Type ComponentClass()
			  {
					throw new System.NotSupportedException( "Not implemented for property type" + name() );
			  }

			  internal object Convert( string value )
			  {
					throw new System.NotSupportedException( "Not implemented for property type" + name() );
			  }

			  internal object Convert( PropType componentType, string value )
			  {
					throw new System.NotSupportedException( "Not implemented for property type" + name() );
			  }

			 public static IList<PropType> values()
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

			 public static PropType valueOf( string name )
			 {
				 foreach ( PropType enumInstance in PropType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static TestData.Producer<java.util.Map<String, org.neo4j.graphdb.Node>> createGraphFor(final GraphHolder holder, final boolean destroy)
		 public static TestData.Producer<IDictionary<string, Node>> CreateGraphFor( GraphHolder holder, bool destroy )
		 {
			  return new ProducerAnonymousInnerClass( holder, destroy );
		 }

		 private class ProducerAnonymousInnerClass : TestData.Producer<IDictionary<string, Node>>
		 {
			 private Neo4Net.Test.GraphHolder _holder;
			 private bool _destroy;

			 public ProducerAnonymousInnerClass( Neo4Net.Test.GraphHolder holder, bool destroy )
			 {
				 this._holder = holder;
				 this._destroy = destroy;
			 }

			 public IDictionary<string, Node> create( GraphDefinition graph, string title, string documentation )
			 {
				  return graph.Create( _holder.graphdb() );
			 }

			 public void destroy( IDictionary<string, Node> product, bool successful )
			 {
				  if ( _destroy )
				  {
						GraphDescription.Destroy( product );
				  }
			 }
		 }

		 public override IDictionary<string, Node> Create( GraphDatabaseService graphdb )
		 {
			  IDictionary<string, Node> result = new Dictionary<string, Node>();
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					graphdb.Index().RelationshipAutoIndexer.Enabled = _autoIndexRelationships;
					foreach ( NODE def in _nodes )
					{
						 Node node = Init( graphdb.CreateNode(), def.setNameProperty() ? def.name() : null, def.properties(), graphdb.Index().NodeAutoIndexer, _autoIndexNodes );
						 foreach ( LABEL label in def.labels() )
						 {
							  node.AddLabel( label( label.value() ) );
						 }
						 result[def.name()] = node;
					}
					foreach ( REL def in _rels )
					{
						 Init( result[def.start()].CreateRelationshipTo(result[def.end()], RelationshipType.withName(def.type())), def.setNameProperty() ? def.name() : null, def.properties(), graphdb.Index().RelationshipAutoIndexer, _autoIndexRelationships );
					}
					tx.Success();
			  }
			  return result;
		 }

		 private static T Init<T>( T entity, string name, PROP[] properties, AutoIndexer<T> autoindex, bool auto ) where T : Neo4Net.Graphdb.PropertyContainer
		 {
			  autoindex.Enabled = auto;
			  foreach ( PROP prop in properties )
			  {
					if ( auto )
					{
						 autoindex.StartAutoIndexingProperty( prop.key() );
					}
					PropType tpe = prop.type();
					switch ( tpe.innerEnumValue )
					{
					case Neo4Net.Test.GraphDescription.PropType.InnerEnum.ARRAY:
						 entity.setProperty( prop.key(), tpe.convert(prop.componentType(), prop.value()) );
						 break;
					default:
						 entity.setProperty( prop.key(), prop.type().convert(prop.value()) );
					 break;
					}
			  }
			  if ( !string.ReferenceEquals( name, null ) )
			  {
					if ( auto )
					{
						 autoindex.StartAutoIndexingProperty( "name" );
					}
					entity.setProperty( "name", name );
			  }

			  return entity;
		 }

		 private static readonly PROP[] _noProps = new PROP[] {};
		 private static readonly NODE[] _noNodes = new NODE[] {};
		 private static readonly REL[] _noRels = new REL[] {};
		 private static readonly GraphDescription EMPTY = new GraphDescriptionAnonymousInnerClass( _noNodes, _noRels );

		 private class GraphDescriptionAnonymousInnerClass : GraphDescription
		 {
			 public GraphDescriptionAnonymousInnerClass( NODE[] noNodes, REL[] noRels ) : base( noNodes, noRels, false, false )
			 {
			 }

			 public override IDictionary<string, Node> create( GraphDatabaseService graphdb )
			 {
				  // don't bother with creating a transaction
				  return new Dictionary<string, Node>();
			 }
		 }
		 private readonly NODE[] _nodes;
		 private readonly REL[] _rels;
		 private readonly bool _autoIndexRelationships;
		 private readonly bool _autoIndexNodes;

		 public static GraphDescription Create( params string[] definition )
		 {
			  IDictionary<string, NODE> nodes = new Dictionary<string, NODE>();
			  IList<REL> relationships = new List<REL>();
			  Parse( definition, nodes, relationships );
			  return new GraphDescription( nodes.Values.toArray( _noNodes ), relationships.toArray( _noRels ), false, false );
		 }

		 public static void Destroy( IDictionary<string, Node> nodes )
		 {
			  if ( nodes.Count == 0 )
			  {
					return;
			  }
			  GraphDatabaseService db = nodes.Values.GetEnumerator().next().GraphDatabase;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 foreach ( Relationship rel in node.Relationships )
						 {
							  rel.Delete();
						 }
						 node.Delete();
					}
					tx.Success();
			  }
		 }

		 public static GraphDescription Create( Graph graph )
		 {
			  if ( graph == null )
			  {
					return EMPTY;
			  }
			  IDictionary<string, NODE> nodes = new Dictionary<string, NODE>();
			  foreach ( NODE node in graph.nodes() )
			  {
					if ( nodes.put( Defined( node.name() ), node ) != null )
					{
						 throw new System.ArgumentException( "Node \"" + node.name() + "\" defined more than once" );
					}
			  }
			  IDictionary<string, REL> rels = new Dictionary<string, REL>();
			  IList<REL> relationships = new List<REL>();
			  foreach ( REL rel in graph.relationships() )
			  {
					CreateIfAbsent( nodes, rel.start() );
					CreateIfAbsent( nodes, rel.end() );
					string name = rel.name();
					if ( !name.Equals( "" ) )
					{
						 if ( rels.put( name, rel ) != null )
						 {
							  throw new System.ArgumentException( "Relationship \"" + name + "\" defined more than once" );
						 }
					}
					relationships.Add( rel );
			  }
			  Parse( graph.value(), nodes, relationships );
			  return new GraphDescription( nodes.Values.toArray( _noNodes ), relationships.toArray( _noRels ), graph.autoIndexNodes(), graph.autoIndexRelationships() );
		 }

		 private static void CreateIfAbsent( IDictionary<string, NODE> nodes, string name, params string[] labels )
		 {
			  if ( !nodes.ContainsKey( name ) )
			  {
					nodes[name] = new DefaultNode( name, labels );
			  }
			  else
			  {
					NODE preexistingNode = nodes[name];
					// Join with any new labels
					HashSet<string> joinedLabels = new HashSet<string>( asList( labels ) );
					foreach ( LABEL label in preexistingNode.labels() )
					{
						 joinedLabels.Add( label.value() );
					}

					string[] labelNameArray = joinedLabels.toArray( new string[joinedLabels.Count] );
					nodes[name] = new NodeWithAddedLabels( preexistingNode, labelNameArray );
			  }
		 }

		 private static string ParseAndCreateNodeIfAbsent( IDictionary<string, NODE> nodes, string descriptionToParse )
		 {
			  string[] parts = descriptionToParse.Split( ":", true );
			  if ( parts.Length == 0 )
			  {
					throw new System.ArgumentException( "Empty node names are not allowed." );
			  }

			  CreateIfAbsent( nodes, parts[0], copyOfRange( parts, 1, parts.Length ) );
			  return parts[0];

		 }

		 private static void Parse( string[] description, IDictionary<string, NODE> nodes, IList<REL> relationships )
		 {
			  foreach ( string part in description )
			  {
					foreach ( string line in part.Split( "\n", true ) )
					{
						 string[] components = line.Split( " ", true );
						 if ( components.Length != 3 )
						 {
							  throw new System.ArgumentException( "syntax error: \"" + line + "\"" );
						 }

						 string startName = ParseAndCreateNodeIfAbsent( nodes, Defined( components[0] ) );
						 string endName = ParseAndCreateNodeIfAbsent( nodes, Defined( components[2] ) );
						 relationships.Add( new DefaultRel( startName, components[1], endName ) );
					}
			  }
		 }

		 private GraphDescription( NODE[] nodes, REL[] rels, bool autoIndexNodes, bool autoIndexRelationships )
		 {
			  this._nodes = nodes;
			  this._rels = rels;
			  this._autoIndexNodes = autoIndexNodes;
			  this._autoIndexRelationships = autoIndexRelationships;
		 }

		 internal static string Defined( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) || name.Equals( "" ) )
			  {
					throw new System.ArgumentException( "Node name not provided" );
			  }
			  return name;
		 }

		 private class Default
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;

			  internal Default( string name )
			  {
					this.NameConflict = "".Equals( name ) ? null : name;
			  }

			  public virtual string Name()
			  {
					return NameConflict;
			  }

			  public virtual Type AnnotationType()
			  {
					throw new System.NotSupportedException( "this is not a real annotation" );
			  }

			  public virtual PROP[] Properties()
			  {
					return _noProps;
			  }

			  public virtual bool SetNameProperty()
			  {
					return true;
			  }
		 }

		 private class DefaultNode : Default, NODE
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LABEL[] LabelsConflict;

			  internal DefaultNode( string name, string[] labelNames ) : base( name )
			  {
					LabelsConflict = new LABEL[labelNames.Length];
					for ( int i = 0; i < labelNames.Length; i++ )
					{
						 LabelsConflict[i] = new DefaultLabel( labelNames[i] );
					}
			  }

			  public override LABEL[] Labels()
			  {
					return LabelsConflict;
			  }
		 }

		 /*
		  * Used if the user has defined the same node twice, with different labels, this combines
		  * all labels the user has added.
		  */
		 private class NodeWithAddedLabels : NODE
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LABEL[] LabelsConflict;
			  internal readonly NODE Inner;

			  internal NodeWithAddedLabels( NODE inner, string[] labelNames )
			  {
					this.Inner = inner;
					LabelsConflict = new LABEL[labelNames.Length];
					for ( int i = 0; i < labelNames.Length; i++ )
					{
						 LabelsConflict[i] = new DefaultLabel( labelNames[i] );
					}
			  }

			  public override string Name()
			  {
					return Inner.name();
			  }

			  public override PROP[] Properties()
			  {
					return Inner.properties();
			  }

			  public override LABEL[] Labels()
			  {
					return LabelsConflict;
			  }

			  public override bool SetNameProperty()
			  {
					return Inner.setNameProperty();
			  }

			  public override Type AnnotationType()
			  {
					return Inner.annotationType();
			  }
		 }

		 private class DefaultRel : Default, REL
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string StartConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string TypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string EndConflict;

			  internal DefaultRel( string start, string type, string end ) : base( null )
			  {
					this.TypeConflict = type;
					this.StartConflict = Defined( start );
					this.EndConflict = Defined( end );
			  }

			  public override string Start()
			  {
					return StartConflict;
			  }

			  public override string End()
			  {
					return EndConflict;
			  }

			  public override string Type()
			  {
					return TypeConflict;
			  }
		 }

		 private class DefaultLabel : LABEL
		 {

			  internal readonly string Name;

			  internal DefaultLabel( string name )
			  {

					this.Name = name;
			  }

			  public override Type AnnotationType()
			  {
					throw new System.NotSupportedException( "this is not a real annotation" );
			  }

			  public override string Value()
			  {
					return Name;
			  }
		 }

	}

}