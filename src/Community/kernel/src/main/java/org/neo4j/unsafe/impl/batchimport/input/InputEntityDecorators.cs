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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using Decorator = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator;

	/// <summary>
	/// Common <seealso cref="InputEntityVisitor"/> decorators, able to provide defaults or overrides.
	/// </summary>
	public class InputEntityDecorators
	{
		 private InputEntityDecorators()
		 {
		 }

		 /// <summary>
		 /// Ensures that all input nodes will at least have the given set of labels.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.unsafe.impl.batchimport.input.csv.Decorator additiveLabels(final String[] labelNamesToAdd)
		 public static Decorator AdditiveLabels( string[] labelNamesToAdd )
		 {
			  if ( labelNamesToAdd == null || labelNamesToAdd.Length == 0 )
			  {
					return NoDecorator;
			  }

			  return node => new AdditiveLabelsDecorator( node, labelNamesToAdd );
		 }

		 /// <summary>
		 /// Ensures that input relationships without a specified relationship type will get
		 /// the specified default relationship type.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.unsafe.impl.batchimport.input.csv.Decorator defaultRelationshipType(final String defaultType)
		 public static Decorator DefaultRelationshipType( string defaultType )
		 {
			  return string.ReferenceEquals( defaultType, null ) ? NoDecorator : relationship => new RelationshipTypeDecorator( relationship, defaultType );
		 }

		 private sealed class AdditiveLabelsDecorator : InputEntityVisitor_Delegate
		 {
			  internal readonly string[] Transport = new string[1];
			  internal readonly string[] LabelNamesToAdd;
			  internal readonly bool[] SeenLabels;
			  internal bool SeenLabelField;

			  internal AdditiveLabelsDecorator( InputEntityVisitor actual, string[] labelNamesToAdd ) : base( actual )
			  {
					this.LabelNamesToAdd = labelNamesToAdd;
					this.SeenLabels = new bool[labelNamesToAdd.Length];
			  }

			  public override bool LabelField( long labelField )
			  {
					SeenLabelField = true;
					return base.LabelField( labelField );
			  }

			  public override bool Labels( string[] labels )
			  {
					if ( !SeenLabelField )
					{
						 foreach ( string label in labels )
						 {
							  for ( int i = 0; i < LabelNamesToAdd.Length; i++ )
							  {
									if ( !SeenLabels[i] && LabelNamesToAdd[i].Equals( label ) )
									{
										 SeenLabels[i] = true;
									}
							  }
						 }
					}
					return base.Labels( labels );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endOfEntity() throws java.io.IOException
			  public override void EndOfEntity()
			  {
					if ( !SeenLabelField )
					{
						 for ( int i = 0; i < SeenLabels.Length; i++ )
						 {
							  if ( !SeenLabels[i] )
							  {
									Transport[0] = LabelNamesToAdd[i];
									base.Labels( Transport );
							  }
						 }
					}

					Arrays.fill( SeenLabels, false );
					SeenLabelField = false;
					base.EndOfEntity();
			  }
		 }

		 private sealed class RelationshipTypeDecorator : InputEntityVisitor_Delegate
		 {
			  internal readonly string DefaultType;
			  internal bool HasType;

			  internal RelationshipTypeDecorator( InputEntityVisitor actual, string defaultType ) : base( actual )
			  {
					this.DefaultType = defaultType;
			  }

			  public override bool Type( int type )
			  {
					HasType = true;
					return base.Type( type );
			  }

			  public override bool Type( string type )
			  {
					if ( !string.ReferenceEquals( type, null ) )
					{
						 HasType = true;
					}
					return base.Type( type );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void endOfEntity() throws java.io.IOException
			  public override void EndOfEntity()
			  {
					if ( !HasType )
					{
						 base.Type( DefaultType );
						 HasType = false;
					}

					base.EndOfEntity();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.unsafe.impl.batchimport.input.csv.Decorator decorators(final org.neo4j.unsafe.impl.batchimport.input.csv.Decorator... decorators)
		 public static Decorator Decorators( params Decorator[] decorators )
		 {
			  return new DecoratorAnonymousInnerClass( decorators );
		 }

		 private class DecoratorAnonymousInnerClass : Decorator
		 {
			 private Decorator[] _decorators;

			 public DecoratorAnonymousInnerClass( Decorator[] decorators )
			 {
				 this._decorators = decorators;
			 }

			 public InputEntityVisitor apply( InputEntityVisitor from )
			 {
				  foreach ( Decorator decorator in _decorators )
				  {
						from = decorator.apply( from );
				  }
				  return from;
			 }

			 public bool Mutable
			 {
				 get
				 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					  return Stream.of( _decorators ).anyMatch( Decorator::isMutable );
				 }
			 }
		 }

		 public static readonly Decorator NoDecorator = value => value;
	}

}