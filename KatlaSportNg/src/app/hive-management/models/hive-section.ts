export class HiveSection {
    constructor( 
        public id: number,
        public name: string,
        public storeHiveId: string,
        public code: string,
        public isDeleted: boolean,
        public lastUpdated: string) { }
}
